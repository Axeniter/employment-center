from fastapi import APIRouter, Depends, HTTPException, status, Query, WebSocket, WebSocketDisconnect
from sqlalchemy.ext.asyncio import AsyncSession
from typing import List, Optional, Tuple
from uuid import UUID
from core.database import get_db
from core.dependencies import get_current_active_user, require_role
from orm.chat import (
    get_message_by_id,
    create_message,
    get_conversation_messages,
    get_user_conversations
)
from schemas.chat import (
    MessageCreate,
    MessageResponse
)
from models.user import User

class ConnectionManager:
    def __init__(self):
        self.active_connections: dict[UUID, WebSocket] = {}
    
    async def connect(self, websocket: WebSocket, user_id: UUID):
        await websocket.accept()
        self.active_connections[user_id] = websocket
    
    def disconnect(self, user_id: UUID):
        if user_id in self.active_connections:
            del self.active_connections[user_id]
    
    async def send_personal_message(self, message: dict, user_id: UUID):
        if user_id in self.active_connections:
            websocket = self.active_connections[user_id]
            await websocket.send_json(message)
    
    async def broadcast(self, message: dict, exclude_user: Optional[UUID] = None):
        for user_id, websocket in self.active_connections.items():
            if exclude_user and user_id == exclude_user:
                continue
            try:
                await websocket.send_json(message)
            except Exception:
                pass

manager = ConnectionManager()
chat_router = APIRouter(prefix="/chat", tags=["chat"])

@chat_router.websocket("/ws/{user_id}")
async def websocket_endpoint(websocket: WebSocket, user_id: UUID):
    await manager.connect(websocket, user_id)
    try:
        while True:
            data = await websocket.receive_json()
            
            if data.get("type") == "message":
                receiver_id = UUID(data.get("receiver_id"))
                message_data = {
                    "type": "new_message",
                    "sender_id": str(user_id),
                    "content": data.get("content"),
                    "timestamp": data.get("timestamp")
                }
                await manager.send_personal_message(message_data, receiver_id)
                
    except WebSocketDisconnect:
        manager.disconnect(user_id)


@chat_router.get("/conversations", response_model=List[Tuple[UUID, MessageResponse]])
async def get_my_conversations(user: User = Depends(get_current_active_user), db: AsyncSession = Depends(get_db)):
    conversations = await get_user_conversations(db, user.id)
    return conversations

@chat_router.get("/conversation/{other_user_id}", response_model=List[MessageResponse])
async def get_conversation(other_user_id: UUID, skip: int = Query(0, ge=0, description="Number of messages to skip"),\
                           limit: int = Query(50, ge=1, le=200, description="Number of messages to return"),
                           user: User = Depends(get_current_active_user), db: AsyncSession = Depends(get_db)):
    messages = await get_conversation_messages(db, user.id, other_user_id, skip, limit)
    return messages

@chat_router.post("/message", response_model=MessageResponse, status_code=status.HTTP_201_CREATED)
async def send_message(message: MessageCreate, user = Depends(get_current_active_user), db: AsyncSession = Depends(get_db)):
    if user.id == message.receiver_id:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="You cannot send messages to yourself"
        )
    
    created_message = await create_message(db, message, user.id)
    
    websocket_message = {
        "type": "new_message",
        "message_id": created_message.id,
        "sender_id": str(user.id),
        "content": created_message.content,
        "created_at": created_message.created_at.isoformat()
    }
    await manager.send_personal_message(websocket_message, message.receiver_id)
    
    return created_message

@chat_router.get("/message/{message_id}", response_model=MessageResponse)
async def get_message(message_id: int, user: User = Depends(get_current_active_user),
                      db: AsyncSession = Depends(get_db)):
    message = await get_message_by_id(db, message_id)
    if not message:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Message not found"
        )
    
    if user.id not in [message.sender_id, message.receiver_id]:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="You don't have permission to view this message")
    
    return message