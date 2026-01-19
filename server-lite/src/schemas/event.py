from pydantic import BaseModel, Field, field_validator
from typing import Optional
from uuid import UUID
from datetime import datetime
import re

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

class EventSearch(BaseModel):
    search_text: Optional[str] = None
    
    location: Optional[str] = None
    is_remote: Optional[bool] = None
    
    date_from: Optional[datetime] = None
    date_to: Optional[datetime] = None
    
    sort_by: Optional[str] = Field(default=None,
        description="Sort: date_asc, date_desc")
    
    @field_validator('sort_by')
    @classmethod
    def validate_sort_by(cls, v):
        if v is None:
            return v
        
        pattern = r'^(date)_(asc|desc)$'
        if not re.match(pattern, v):
            raise ValueError('sort_by must be: field_order (date_asc, date_desc)')
        return v
    
    class Config:
        from_attributes = True