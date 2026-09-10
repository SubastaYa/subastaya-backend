#!/bin/bash
# Script para probar el Optimistic Locking (HTTP 409 Conflict)
# NOTA: Es necesario reemplazar TOKEN_USER_1 y TOKEN_USER_2 con los tokens JWT reales generados en el ambiente de prueba.

URL="http://localhost:5000/api/auctions/1/bids"
TOKEN_USER_1="EL_TOKEN_DEL_COMPRADOR_1"
TOKEN_USER_2="EL_TOKEN_DEL_COMPRADOR_2"
PAYLOAD='{"amount": 100000}'

# Función para enviar peticiones en background (simultáneas)
send_bid() {
    curl -s -o /dev/null -w "Respuesta HTTP: %{http_code}\n" -X POST "$URL" \
    -H "Authorization: Bearer $1" \
    -H "Content-Type: application/json" \
    -d "$PAYLOAD" &
}

echo "Lanzando 2 peticiones simultáneas idénticas..."
send_bid $TOKEN_USER_1
send_bid $TOKEN_USER_2

wait
echo "Test finalizado. Una debió responder 200 y la otra 409."
