# Domain Model

## Entities

### User
| Field        | Type      | Notes                              |
|--------------|-----------|------------------------------------|
| Id           | Guid (PK) |                                    |
| Email        | string    | unique                             |
| PasswordHash | string    |                                    |
| FullName     | string    |                                    |
| Role         | UserRole  | enum: Member, Moderator, Admin     |
| CreatedAt    | DateTime  |                                    |

> Any `Member` can both list gear and buy gear — there is no fixed Buyer / Seller split.

---

### Category
| Field | Type      | Notes  |
|-------|-----------|--------|
| Id    | Guid (PK) |        |
| Name  | string    |        |
| Slug  | string    | unique |

---

### Listing
| Field               | Type          | Notes                                                              |
|---------------------|---------------|--------------------------------------------------------------------|
| Id                  | Guid (PK)     |                                                                    |
| Title               | string        |                                                                    |
| Description         | string        |                                                                    |
| CategoryId          | Guid (FK)     | → Category                                                         |
| SellerId            | Guid (FK)     | → User                                                             |
| Price               | Money         | value object — amount + currency                                   |
| Condition           | GearCondition | enum: New, LikeNew, Good, Fair, Poor                               |
| Status              | ListingStatus | see state machine below                                            |
| ManufactureDate     | DateOnly?     | nullable — not all gear has a visible manufacture date             |
| SafetyCertification | string?       | e.g. "CE EN 892", "UIAA 101", "EN 12492"                          |
| Location            | string        |                                                                    |
| ViewsCount          | int           |                                                                    |
| IsPromoted          | bool          | set by Promotions module via domain event                          |
| CreatedAt           | DateTime      |                                                                    |
| UpdatedAt           | DateTime      |                                                                    |
| ExpiresAt           | DateTime      | 60 days from activation by default                                 |

> `Listing` is an aggregate root. It owns the `ListingImage` collection — images are never accessed independently of their listing.

---

### ListingImage
| Field      | Type      | Notes                                      |
|------------|-----------|--------------------------------------------|
| Id         | Guid (PK) |                                            |
| ListingId  | Guid (FK) | → Listing                                  |
| StorageKey | string    | object key in MinIO / S3                   |
| SortOrder  | int       |                                            |
| IsPrimary  | bool      | exactly one image per listing must be true |

---

### Conversation
| Field         | Type      | Notes                                          |
|---------------|-----------|------------------------------------------------|
| Id            | Guid (PK) |                                                |
| ListingId     | Guid (FK) | → Listing                                      |
| BuyerId       | Guid (FK) | → User                                         |
| SellerId      | Guid (FK) | → User                                         |
| CreatedAt     | DateTime  |                                                |
| LastMessageAt | DateTime? |                                                |

> One conversation per `(Listing, Buyer)` pair — a buyer cannot open a second thread about the same listing.  
> `Conversation` is an aggregate root. It owns the `Message` collection.

---

### Message
| Field          | Type      | Notes                                     |
|----------------|-----------|-------------------------------------------|
| Id             | Guid (PK) |                                           |
| ConversationId | Guid (FK) | → Conversation                            |
| SenderId       | Guid (FK) | → User                                    |
| Body           | string    |                                           |
| SentAt         | DateTime  |                                           |
| ReadAt         | DateTime? | null until the recipient reads it         |

---

### Report
| Field              | Type         | Notes                                      |
|--------------------|--------------|--------------------------------------------|
| Id                 | Guid (PK)    |                                            |
| ListingId          | Guid (FK)    | → Listing                                  |
| ReportedByUserId   | Guid (FK)    | → User                                     |
| Reason             | ReportReason | enum: Counterfeit, Prohibited, Scam, SafetyConcern, Other |
| Description        | string?      |                                            |
| Status             | ReportStatus | enum: Pending, Reviewed, Dismissed         |
| ReviewedByUserId   | Guid? (FK)   | → User (Moderator or Admin), null until reviewed |
| ReviewedAt         | DateTime?    |                                            |

---

### Promotion
| Field                 | Type            | Notes                                       |
|-----------------------|-----------------|---------------------------------------------|
| Id                    | Guid (PK)       |                                             |
| ListingId             | Guid (FK)       | → Listing                                   |
| Tier                  | PromotionTier   | enum: Standard, Featured                    |
| StartAt               | DateTime        |                                             |
| EndAt                 | DateTime        |                                             |
| Price                 | Money           | value object — amount + currency            |
| PaymentStatus         | PaymentStatus   | enum: Pending, Completed, Failed, Refunded  |
| StripePaymentIntentId | string?         | null until Stripe confirms                  |

> When `PaymentStatus` transitions to `Completed`, a `PromotionActivatedEvent` domain event is raised. The Listings module handles it and flips `IsPromoted = true` on the relevant listing.

---

## Value Objects

### Money
| Field    | Type    |
|----------|---------|
| Amount   | decimal |
| Currency | string  | ISO 4217, e.g. "PLN", "EUR", "USD" |

### GearCertification
Wraps a raw certification string and validates it against known standards (EN 892, EN 12492, UIAA 101, etc.). Prevents free-text garbage from being stored as a certification claim.

---

## Enums

### UserRole
`Member` | `Moderator` | `Admin`

### ListingStatus
`Draft` | `Active` | `Reserved` | `Sold` | `Expired` | `Removed`

### GearCondition
`New` | `LikeNew` | `Good` | `Fair` | `Poor`

### ReportReason
`Counterfeit` | `Prohibited` | `Scam` | `SafetyConcern` | `Other`

### ReportStatus
`Pending` | `Reviewed` | `Dismissed`

### PromotionTier
`Standard` | `Featured`

### PaymentStatus
`Pending` | `Completed` | `Failed` | `Refunded`

---

## Listing State Machine

Allowed status transitions:

| From     | To (allowed)         | Who can trigger               |
|----------|----------------------|-------------------------------|
| Draft    | Active               | Seller                        |
| Active   | Reserved             | Seller (marks as pending sale)|
| Active   | Sold                 | Seller                        |
| Active   | Expired              | System (background job)       |
| Active   | Removed              | Moderator / Admin only        |
| Reserved | Sold                 | Seller                        |
| Reserved | Active               | Seller (deal fell through)    |
| Expired  | Active               | Seller (renew listing)        |
| Sold     | —                    | terminal state                |
| Removed  | —                    | terminal state                |

Any transition not listed above must be rejected with a domain exception (`InvalidListingStatusTransitionException`).

---

## Domain Events

| Event                     | Raised by           | Handled by                                           |
|---------------------------|---------------------|------------------------------------------------------|
| `ListingPublishedEvent`   | Listings module     | —                                                    |
| `ListingSoldEvent`        | Listings module     | —                                                    |
| `ListingRemovedEvent`     | Moderation module   | Listings module (sets status = Removed)              |
| `MessageSentEvent`        | Chat module         | — (SignalR push handled in-process by the hub)       |
| `PromotionActivatedEvent` | Promotions module   | Listings module (sets IsPromoted = true)             |
| `PromotionExpiredEvent`   | System (background) | Listings module (sets IsPromoted = false)            |
