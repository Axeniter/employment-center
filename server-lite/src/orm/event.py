from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
from sqlalchemy.orm import selectinload
from typing import List, Optional
from models.event import Event
from schemas.event import EventCreate, EventUpdate
from datetime import datetime
from uuid import UUID

async def get_event_by_id(db: AsyncSession, event_id: int) -> Optional[Event]:
    result = await db.execute(select(Event).where(Event.id == event_id))
    return result.scalar_one_or_none()


async def get_events_by_employer(db: AsyncSession, employer_id: UUID) -> List[Event]:
    result = await db.execute(select(Event).where(Event.employer_id == employer_id))
    return result.scalars().all()


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
    return result.scalars().all()