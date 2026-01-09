from pydantic import BaseModel
from typing import Optional
from uuid import UUID
from datetime import datetime

class EventBase(BaseModel):
    title: str
    description: str
    location: Optional[str] = None
    is_remote: bool = False
    date: datetime

class EventCreate(EventBase):
    pass

class EventUpdate(BaseModel):
    title: Optional[str] = None
    description: Optional[str] = None
    location: Optional[str] = None
    is_remote: Optional[bool] = None
    date: Optional[datetime] = None

class EventResponse(EventBase):
    id: int
    employer_id: UUID
    is_active: bool

    class Config:
        from_attributes = True