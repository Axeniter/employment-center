from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.future import select
from sqlalchemy import desc, or_, and_, func
from sqlalchemy.orm import joinedload
from typing import List, Optional, Tuple
from uuid import UUID
from schemas.chat import MessageCreate
from models.chat import Message

async def get_message_by_id(db: AsyncSession, message_id: int) -> Optional[Message]:
    result = await db.execute(
        select(Message)
        .options(
            joinedload(Message.sender),
            joinedload(Message.receiver)
        )
        .filter(Message.id == message_id)
    )
    return result.scalar_one_or_none()


async def create_message(db: AsyncSession, message: MessageCreate, sender_id: UUID) -> Message:
    db_message = Message(sender_id=sender_id, **message.model_dump())
    db.add(db_message)
    await db.commit()
    await db.refresh(db_message)
    return db_message


async def get_conversation_messages(db: AsyncSession, user1_id: UUID, user2_id: UUID,
                                    skip: int = 0, limit: int = 50) -> List[Message]:
    result = await db.execute(
        select(Message)
        .options(
            joinedload(Message.sender),
            joinedload(Message.receiver)
        )
        .where(
            or_(
                and_(Message.sender_id == user1_id, Message.receiver_id == user2_id),
                and_(Message.sender_id == user2_id, Message.receiver_id == user1_id)
            )
        )
        .order_by(desc(Message.created_at))
        .offset(skip)
        .limit(limit)
    )
    return result.scalars().all()


async def get_user_conversations(db: AsyncSession, user_id: UUID) -> List[Tuple[UUID, Message]]:
    subquery = (
        select(
            Message.receiver_id.label("other_user_id"),
            func.max(Message.created_at).label("last_message_time")
        )
        .where(Message.sender_id == user_id)
        .group_by(Message.receiver_id)
        .union_all(
            select(
                Message.sender_id.label("other_user_id"),
                func.max(Message.created_at).label("last_message_time")
            )
            .where(Message.receiver_id == user_id)
            .group_by(Message.sender_id)
        )
        .subquery()
    )
    
    last_messages_query = (
        select(Message)
        .options(
            joinedload(Message.sender),
            joinedload(Message.receiver)
        )
        .where(
            or_(
                and_(Message.sender_id == user_id, Message.receiver_id == subquery.c.other_user_id),
                and_(Message.receiver_id == user_id, Message.sender_id == subquery.c.other_user_id)
            ),
            Message.created_at == subquery.c.last_message_time
        )
        .order_by(desc(Message.created_at))
    )
    
    result = await db.execute(last_messages_query)
    return result.scalars().all()