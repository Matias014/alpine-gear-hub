#!/usr/bin/env bash
# Seeds 10 published demo listings (one per gear category), each with a photo from
# ./seed-images, spread across 3 demo seller accounts so testers can see ownership checks,
# messaging between different sellers, etc. Safe to re-run - it just creates a fresh batch of
# listings each time under the same 3 accounts (registration is a no-op if they already exist).
#
# Requirements: backend running (docker compose up + dotnet run), curl, jq.
# Usage: ./scripts/seed-demo-listings.sh [API_BASE_URL]
set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
IMAGES_DIR="$SCRIPT_DIR/seed-images"
API="${1:-http://localhost:8080/api}"
PASSWORD="Demo1234!"

# name | email
SELLERS=(
  "Alex Sterling|alex.sterling@alpinegearhub.local"
  "Mia Larsen|mia.larsen@alpinegearhub.local"
  "Sam Rivera|sam.rivera@alpinegearhub.local"
)

echo "== Registering (or logging in as) demo sellers =="
declare -A SELLER_TOKEN
for entry in "${SELLERS[@]}"; do
  IFS='|' read -r full_name email <<< "$entry"

  register_response=$(curl -s -w "\n%{http_code}" -X POST "$API/auth/register" \
    -H "Content-Type: application/json" \
    -d "{\"fullName\":\"$full_name\",\"email\":\"$email\",\"password\":\"$PASSWORD\"}")
  register_status=$(echo "$register_response" | tail -n1)
  register_body=$(echo "$register_response" | sed '$d')

  if [ "$register_status" = "201" ] || [ "$register_status" = "200" ]; then
    token=$(echo "$register_body" | jq -r '.accessToken')
  else
    login_response=$(curl -s -X POST "$API/auth/login" \
      -H "Content-Type: application/json" \
      -d "{\"email\":\"$email\",\"password\":\"$PASSWORD\"}")
    token=$(echo "$login_response" | jq -r '.accessToken')
  fi

  if [ -z "$token" ] || [ "$token" = "null" ]; then
    echo "Could not obtain an access token for $email. Is the backend running at $API? Aborting."
    exit 1
  fi

  SELLER_TOKEN["$email"]="$token"
  echo "  $full_name <$email> ready"
done

echo "== Fetching categories =="
CATEGORIES_JSON=$(curl -s "$API/categories")

category_id() {
  echo "$CATEGORIES_JSON" | jq -r ".[] | select(.slug == \"$1\") | .id"
}

# seller email | slug | image file | title | description | price | currency | condition | location
LISTINGS=(
  "alex.sterling@alpinegearhub.local|ropes|Ropes.jpg|Mammut 60m Dynamic Rope|Barely used, one season of top-roping and sport climbing. No falls taken on lead, stored dry.|89.99|EUR|Good|Chamonix, France"
  "alex.sterling@alpinegearhub.local|harnesses|Harnesses.jpg|Petzl Corax Harness|Comfortable all-round harness, adjustable leg loops, great for a first harness.|45.00|EUR|LikeNew|Innsbruck, Austria"
  "alex.sterling@alpinegearhub.local|helmets|Helmets.jpg|Black Diamond Vapor Helmet|Ultralight climbing helmet, no impacts, minor cosmetic scuffs from storage.|55.00|EUR|Good|Grenoble, France"
  "alex.sterling@alpinegearhub.local|carabiners-quickdraws|Carabiners_and_Quickdraws.jpg|DMM Quickdraw Set (6-pack)|Set of six sport-climbing quickdraws, wear-indicator dogbones still bright.|60.00|EUR|Good|Sheffield, UK"
  "mia.larsen@alpinegearhub.local|ice-axes-crampons|Ice_Axes_and_Crampons.jpg|Petzl Sum'Tec Ice Axe + Crampons|Technical ice axe paired with steel crampons, a few seasons of alpine use.|120.00|EUR|Fair|Zermatt, Switzerland"
  "mia.larsen@alpinegearhub.local|boots-shoes|Boots_and_Shoes.jpg|La Sportiva Nepal Cube GTX Boots|Insulated mountaineering boots, resoled once, plenty of life left.|150.00|EUR|Good|Chamonix, France"
  "mia.larsen@alpinegearhub.local|backpacks|Backpacks.jpg|Osprey Mutant 38L Backpack|Streamlined alpine climbing pack, ice-axe loops, used for a handful of trips.|70.00|EUR|LikeNew|Bergen, Norway"
  "sam.rivera@alpinegearhub.local|tents-shelters|Tents_and_Shelters.jpg|MSR Access 2 Tent|4-season 2-person tent, seam-sealed, pitched fewer than ten nights.|250.00|EUR|Good|Innsbruck, Austria"
  "sam.rivera@alpinegearhub.local|clothing|Clothing.jpg|Arc'teryx Alpha SV Jacket|Fully waterproof hardshell, no delamination, zippers all smooth.|180.00|EUR|Good|Chamonix, France"
  "sam.rivera@alpinegearhub.local|other|Other.jpg|Alpine Route Topo Map + Locking Carabiner|Bundle: a laminated regional topo map and a spare locking carabiner.|15.00|EUR|New|Grenoble, France"
)

echo "== Creating, publishing, and photographing listings =="
for entry in "${LISTINGS[@]}"; do
  IFS='|' read -r seller_email slug image title description price currency condition location <<< "$entry"
  token="${SELLER_TOKEN[$seller_email]}"
  cat_id=$(category_id "$slug")

  if [ -z "$cat_id" ]; then
    echo "  [$slug] category not found, skipping"
    continue
  fi

  image_path="$IMAGES_DIR/$image"
  if [ ! -f "$image_path" ]; then
    echo "  [$slug] image not found at $image_path, skipping"
    continue
  fi

  create_body=$(jq -n \
    --arg categoryId "$cat_id" \
    --arg title "$title" \
    --arg description "$description" \
    --arg currency "$currency" \
    --arg condition "$condition" \
    --arg location "$location" \
    --argjson price "$price" \
    --arg sellerId "00000000-0000-0000-0000-000000000000" \
    '{categoryId: $categoryId, title: $title, description: $description, price: $price, currency: $currency, condition: $condition, location: $location, sellerId: $sellerId}')

  listing_response=$(curl -s -X POST "$API/listings" \
    -H "Authorization: Bearer $token" \
    -H "Content-Type: application/json" \
    -d "$create_body")
  listing_id=$(echo "$listing_response" | jq -r '.id')

  if [ -z "$listing_id" ] || [ "$listing_id" = "null" ]; then
    echo "  [$slug] failed to create listing: $listing_response"
    continue
  fi

  curl -s -X POST "$API/listings/$listing_id/publish" -H "Authorization: Bearer $token" > /dev/null

  curl -s -X POST "$API/listings/$listing_id/images" \
    -H "Authorization: Bearer $token" \
    -F "file=@${image_path};type=image/jpeg" > /dev/null

  echo "  [$slug] created listing $listing_id — \"$title\" (seller: $seller_email)"
done

echo "== Done =="
echo "Demo seller accounts (password for all: $PASSWORD):"
for entry in "${SELLERS[@]}"; do
  IFS='|' read -r full_name email <<< "$entry"
  echo "  $full_name — $email"
done
