#!/usr/bin/env bash
set -x

SCRIPTDIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" >/dev/null && pwd)"

USERNAME=${USERNAME:-u$(date +%s)}
EMAIL=${EMAIL:-$USERNAME@mail.com}
PASSWORD=${PASSWORD:-password}

npx newman run $SCRIPTDIR/test.postman_collection.json \
  --delay-request 500 \
  --global-var "APIURL=localhost:5000/api" \
  --global-var "USERNAME=$USERNAME" \
  --global-var "EMAIL=$EMAIL" \
  --global-var "PASSWORD=$PASSWORD" \
  "$@"
