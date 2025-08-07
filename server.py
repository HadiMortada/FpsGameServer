import asyncio
import websockets
import json

clients = {}
next_id = 1

async def handle_client(websocket):
    global next_id
    client_id = next_id
    next_id += 1
    clients[client_id] = websocket
    print(f"[Server] Client {client_id} connected.")

    try:
        # Notify the client of its ID
        await websocket.send(json.dumps({"type": "YOUR_ID", "id": client_id}))

        # Main loop
        async for message in websocket:
            try:
                data = json.loads(message)

                # Broadcast POS messages to other clients
                if data["type"] == "POS":
                    for other_id, other_ws in clients.items():
                        if other_id != client_id:
                            await other_ws.send(json.dumps({
                                "type": "POS",
                                "id": client_id,
                                "x": data["x"],
                                "y": data["y"],
                                "z": data["z"]
                            }))
            except Exception as e:
                print(f"[Server] Error parsing message from {client_id}: {e}")

    except websockets.exceptions.ConnectionClosed:
        print(f"[Server] Client {client_id} disconnected.")
    finally:
        del clients[client_id]

async def main():
    print("[Server] Starting WebSocket server on ws://localhost:8765")
    async with websockets.serve(handle_client, "0.0.0.0", 8765):
        await asyncio.Future()  # Run forever

if __name__ == "__main__":
    asyncio.run(main())
