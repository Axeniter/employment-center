from fastapi import APIRouter, Depends, HTTPException, status, Query
from sqlalchemy.ext.asyncio import AsyncSession
from typing import List, Optional
from datetime import datetime
from uuid import UUID
from models.user import UserRole
from core.database import get_db
from core.dependencies import get_current_user, require_role
from models.user import User
from schemas.event import EventCreate, EventUpdate, EventResponse, EventSearch
from orm.event import (
    get_event_by_id,
    get_events_by_employer,
    create_event,
    update_event,
    delete_event,
    get_event_with_employer,
    search_events
)

event_router = APIRouter(prefix="/events", tags=["event"])

@event_router.get("/", response_model=List[EventResponse])
async def search_events_endpoint(search_params: EventSearch = Depends(), page: int = Query(1, ge=1),
                                 limit: int = Query(20, ge=1, le=100), db: AsyncSession = Depends(get_db)):
    events = await search_events(db, search_params, page, limit)
    return events

@event_router.get("/{event_id}", response_model=EventResponse)
async def get_event_by_id_endpoint(event_id: int, db: AsyncSession = Depends(get_db)):
    db_event = await get_event_by_id(db, event_id)
    if not db_event:
        raise HTTPException(status_code=status.HTTP_404_NOT_FOUND, detail="Event not found")
    
    return db_event

@event_router.get("/me", response_model=List[EventResponse])
async def get_my_events_endpoint(user = Depends(require_role(UserRole.EMPLOYER)), db: AsyncSession = Depends(get_db)):
    events = await get_events_by_employer(db, user.id)
    return events

@event_router.get("/employer/{employer_id}", response_model=List[EventResponse])
async def get_events_by_employer_endpoint(employer_id: UUID, db: AsyncSession = Depends(get_db)):
    events = await get_events_by_employer(db, employer_id)
    return events

@event_router.post("/", response_model=EventResponse)
async def create_event_endpoint(event_data: EventCreate, user = Depends(require_role(UserRole.EMPLOYER)),
                                db: AsyncSession = Depends(get_db)):
    if event_data.date <= datetime.now():
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Event's date can't be in the past")
        
    new_event = await create_event(db, event_data, user.id)

    return new_event

@event_router.put("/{event_id}", response_model=EventResponse)
async def update_event_endpoint(event_id: int, update_data: EventUpdate, user = Depends(require_role(UserRole.EMPLOYER)),
                                db: AsyncSession = Depends(get_db)):
    db_event = await get_event_by_id(db, event_id)
    if not db_event:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Event not found")
    if db_event.employer_id != user.id:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="You can only update your own events")
    if update_data.date and update_data.date <= datetime.now():
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="Event's date can't be in the past")
    
    updated_event = await update_event(db, event_id, update_data)
    return updated_event

@event_router.delete("/{event_id}")
async def delete_event_endpoint(event_id: int, user = Depends(require_role(UserRole.EMPLOYER)),
                                db: AsyncSession = Depends(get_db)):
    db_event = await get_event_by_id(db, event_id)
    if not db_event:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Event not found")
    
    if db_event.employer_id != user.id:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="You can only delete your own events")
    
    success = await delete_event(db, event_id)
    if not success:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to delete event")