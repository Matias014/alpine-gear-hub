#!/usr/bin/env bash
# Seeds 10 published demo listings (one per gear category), each with a photo from
# ./seed-images, against a locally running backend. Safe to re-run - it just creates a
# fresh batch of listings each time under the same demo seller account.
#
# Requirements: backend running (docker compose up + dotnet run), curl, jq.
# Usage: ./scripts/seed-demo-listings.sh [API_BASE_URL]
set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
IMAGES_DIR="$SCRIPT_DIR/seed-images"
API="${1:-http://localhost:8080/api}"
EMAIL="demo-seller@alpinegearhub.local"
PASSWORD="Demo1234!"
FULL_NAME="Demo Seller"

echo "== Registering (or logging in as) demo seller =="
REGISTER_RESPONSE=$(curl -s -w "\n%{http_code}" -X POST "$API/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"fullName\":\"$FULL_NAME\",\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")
REGISTER_STATUS=$(echo "$REGISTER_RESPONSE" | tail -n1)
REGISTER_BODY=$(echo "$REGISTER_RESPONSE" | sed '$d')

if [ "$REGISTER_STATUS" = "201" ] || [ "$REGISTER_STATUS" = "200" ]; then
  TOKEN=$(echo "$REGISTER_BODY" | jq -r '.accessToken')
else
  echo "  (already registered, logging in instead)"
  LOGIN_RESPONSE=$(curl -s -X POST "$API/auth/login" \
    -H "Content-Type: application/json" \
    -d "{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}")
  TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.accessToken')
fi

if [ -z "$TOKEN" ] || [ "$TOKEN" = "null" ]; then
  echo "Could not obtain an access token. Is the backend running at $API? Aborting."
  exit 1
fi
echo "  token acquired"

echo "== Fetching categories =="
CATEGORIES_JSON=$(curl -s "$API/categories")

category_id() {
  echo "$CATEGORIES_JSON" | jq -r ".[] | select(.slug == \"$1\") | .id"
}

# slug | image file | title | description | price | currency | condition | location
LISTINGS=(
  "ropes|Ropes.jpg|Mammut 60m Dynamic Rope|Barely used, one season of top-roping and sport climbing. No falls taken on lead, stored dry.|89.99|EUR|Good|Chamonix, France"
  "harnesses|Harnesses.jpg|Petzl Corax Harness|Comfortable all-round harness, adjustable leg loops, great for a first harness.|45.00|EUR|LikeNew|Innsbruck, Austria"
  "helmets|Helmets.jpg|Black Diamond Vapor Helmet|Ultralight climbing helmet, no impacts, minor cosmetic scuffs from storage.|55.00|EUR|Good|Grenoble, France"
  "carabiners-quickdraws|Carabiners_and_Quickdraws.jpg|DMM Quickdraw Set (6-pack)|Set of six sport-climbing quickdraws, wear-indicator dogbones still bright.|60.00|EUR|Good|Sheffield, UK"
  "ice-axes-crampons|Ice_Axes_and_Crampons.jpg|Petzl Sum'Tec Ice Axe + Crampons|Technical ice axe paired with steel crampons, a few seasons of alpine use.|120.00|EUR|Fair|Zermatt, Switzerland"
  "boots-shoes|Boots_and_Shoes.jpg|La Sportiva Nepal Cube GTX Boots|Insulated mountaineering boots, resoled once, plenty of life left.|150.00|EUR|Good|Chamonix, France"
  "backpacks|Backpacks.jpg|Osprey Mutant 38L Backpack|Streamlined alpine climbing pack, ice-axe loops, used for a handful of trips.|70.00|EUR|LikeNew|Bergen, Norway"
  "tents-shelters|Tents_and_Shelters.jpg|MSR Access 2 Tent|4-season 2-person tent, seam-sealed, pitched fewer than ten nights.|250.00|EUR|Good|Innsbruck, Austria"
  "clothing|Clothing.jpg|Arc'teryx Alpha SV Jacket|Fully waterproof hardshell, no delamination, zippers all smooth.|180.00|EUR|Good|Chamonix, France"
  "other|Other.jpg|Alpine Route Topo Map + Locking Carabiner|Bundle: a laminated regional topo map and a spare locking carabiner.|15.00|EUR|New|Grenoble, France"
)

echo "== Creating, publishing, and photographing listings =="
for entry in "${LISTINGS[@]}"; do
  IFS='|' read -r slug image title description price currency condition location <<< "$entry"
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
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d "$create_body")
  listing_id=$(echo "$listing_response" | jq -r '.id')

  if [ -z "$listing_id" ] || [ "$listing_id" = "null" ]; then
    echo "  [$slug] failed to create listing: $listing_response"
    continue
  fi

  curl -s -X POST "$API/listings/$listing_id/publish" -H "Authorization: Bearer $TOKEN" > /dev/null

  curl -s -X POST "$API/listings/$listing_id/images" \
    -H "Authorization: Bearer $TOKEN" \
    -F "file=@${image_path};type=image/jpeg" > /dev/null

  echo "  [$slug] created listing $listing_id — \"$title\""
done

echo "== Done =="
