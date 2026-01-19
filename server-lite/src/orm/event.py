from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, or_, desc, asc
from sqlalchemy.orm import selectinload, joinedload
from typing import List, Optional
from models.event import Event
from schemas.event import EventCreate, EventUpdate, EventSearch
from datetime import datetime
from uuid import UUID

async def get_event_by_id(db: AsyncSession, event_id: int) -> Optional[Event]:
    result = await db.execute(select(Event).where(Event.id == event_id))
    return result.scalar_one_or_none()


async def get_events_by_employer(db: AsyncSession, employer_id: UUID) -> List[Event]:
    result = await db.execute(select(Event).where(Event.employer_id == employer_id))
    return list(result.scalars().all())


async def create_event(db: AsyncSession, event: EventCreate, employer_id: UUID) -> Event:
    db_event = Event(**event.model_dump(), employer_id=employer_id)
    db.add(db_event)
    await db.commit()
    await db.refresh(db_event)
    return db_event


async def update_event(db: AsyncSession, event_id: int, event: EventUpdate) -> Optional[Event]:
    db_event = await get_event_by_id(db, event_id)
    if not db_event:
        return None
    
    update_data = event.model_dump(exclude_unset=True)
    
    for field, value in update_data.items():
        setattr(db_event, field, value)
    
    await db.commit()
    await db.refresh(db_event)
    return db_event


async def delete_event(db: AsyncSession, event_id: int) -> bool:
    db_event = await get_event_by_id(db, event_id)
    if not db_event:
        return False
    
    await db.delete(db_event)
    await db.commit()
    return True


async def get_event_with_employer(db: AsyncSession, event_id: int) -> Optional[Event]:
    result = await db.execute(select(Event).options(selectinload(Event.employer))
        .where(Event.id == event_id))
    return result.scalar_one_or_none()


async def get_events_by_date_range(db: AsyncSession, start_date: datetime, end_date: datetime) -> List[Event]:
    result = await db.execute(select(Event).where(Event.date.between(start_date, end_date)).order_by(Event.date))
    return list(result.scalars().all())

async def search_events(db: AsyncSession, search_params: EventSearch,
                        page: int = 1, limit: int = 20) -> List[Event]:
    query = select(Event).where(Event.is_active == True)
    query = query.options(joinedload(Event.employer))
    
    if search_params.search_text:
        search_text = f"%{search_params.search_text}%"
        query = query.where(or_(Event.title.ilike(search_text),
                Event.description.ilike(search_text),
                Event.location.ilike(search_text)))
    
    if search_params.location:
        query = query.where(Event.location.ilike(f"%{search_params.location}%"))
    
    if search_params.is_remote is not None:
        query = query.where(Event.is_remote == search_params.is_remote)
    
    if search_params.date_from:
        query = query.where(Event.date >= search_params.date_from)
    
    if search_params.date_to:
        query = query.where(Event.date <= search_params.date_to)
    
    if search_params.sort_by:
        sort_field, sort_order = search_params.sort_by.split("_")
        order_func = desc if sort_order == "desc" else asc
        
        if sort_field == "date":
            query = query.order_by(order_func(Event.date))
    else:
        query = query.order_by(Event.date)
    
    offset = (page - 1) * limit
    query = query.offset(offset).limit(limit)
    result = await db.execute(query)
    events = result.scalars().unique().all()
    
    return list(events)