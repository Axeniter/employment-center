from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.future import select
from sqlalchemy import desc
from sqlalchemy.orm import joinedload
from typing import List, Optional
from models.vacancy import Vacancy, Response
from models.user import User
from schemas.vacancy import VacancyCreate, VacancyUpdate
from uuid import UUID

async def get_vacancy_by_id(db: AsyncSession, vacancy_id: int) -> Optional[Vacancy]:
    result = await db.execute(select(Vacancy).filter(Vacancy.id == vacancy_id))
    return result.scalar_one_or_none()

async def get_vacancies(db: AsyncSession, skip: int = 0,
                         limit: int = 100, employer_id: Optional[UUID] = None) -> List[Vacancy]:
    query = select(Vacancy)
    
    if employer_id:
        query = query.where(Vacancy.employer_id == employer_id)
    
    query = query.offset(skip).limit(limit)
    
    result = await db.execute(query)
    return result.scalars().all()

async def create_vacancy(db: AsyncSession, vacancy: VacancyCreate, employer_id: UUID) -> Vacancy:
    db_vacancy = Vacancy(**vacancy.model_dump(), employer_id=employer_id)
    db.add(db_vacancy)
    await db.commit()
    await db.refresh(db_vacancy)
    return db_vacancy

async def update_vacancy(db: AsyncSession, vacancy_id: int, vacancy: VacancyUpdate) -> Optional[Vacancy]:
    db_vacancy = await get_vacancy_by_id(db, vacancy_id)
    if not db_vacancy:
        return None
    
    update_data = vacancy.model_dump(exclude_none=True)
    for field, value in update_data.items():
        setattr(db_vacancy, field, value)
    
    await db.commit()
    await db.refresh(db_vacancy)
    return db_vacancy

async def delete_vacancy(db: AsyncSession, vacancy_id: int) -> bool:
    db_vacancy = await get_vacancy_by_id(db, vacancy_id)
    if not db_vacancy:
        return False
    
    await db.delete(db_vacancy)
    await db.commit()
    return True

async def create_response(db: AsyncSession, applicant_id: UUID, vacancy_id: int) -> Response:
    db_response = Response(applicant_id=applicant_id, vacancy_id=vacancy_id)
    db.add(db_response)
    await db.commit()
    await db.refresh(db_response)
    return db_response

async def get_response_by_id(db: AsyncSession, response_id: int) -> Optional[Response]:
    result = await db.execute(select(Response).options(joinedload(Response.vacancy), 
                            joinedload(Response.applicant)).filter(Response.id == response_id))
    return result.scalar_one_or_none()

async def get_responses_by_vacancy(db: AsyncSession, vacancy_id: int) -> List[Response]:
    result = await db.execute(
        select(Response)
        .options(joinedload(Response.applicant).joinedload(User.applicant_profile))
        .where(Response.vacancy_id == vacancy_id)
        .order_by(desc(Response.created_at))
    )
    return result.scalars().all()

async def get_responses_by_applicant(db: AsyncSession, applicant_id: UUID) -> List[Response]:
    result = await db.execute(
        select(Response)
        .options(joinedload(Response.vacancy))
        .where(Response.applicant_id == applicant_id)
        .order_by(desc(Response.created_at))
    )
    return result.scalars().all()

async def update_response_status(db: AsyncSession, response_id: int, status: str) -> Optional[Response]:
    response = await get_response_by_id(db, response_id)
    if not response:
        return None
    
    response.status = status
    await db.commit()
    await db.refresh(response)
    return response