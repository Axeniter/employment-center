from pydantic import BaseModel
from datetime import datetime
from uuid import UUID

class MessageBase(BaseModel):
    content: str

class MessageCreate(MessageBase):
    response_id: UUID
    receiver_id: UUID

class MessageResponse(MessageBase):
    id: int
    response_id: int
    sender_id: UUID
    receiver_id: UUID
    created_at: datetime
    
    class Config:
        from_attributes = True