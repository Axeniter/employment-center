from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy.future import select
from sqlalchemy import desc, asc, or_
from sqlalchemy.orm import joinedload
from typing import List, Optional, Tuple
from models.vacancy import Vacancy, Response
from models.user import User
from schemas.vacancy import VacancyCreate, VacancyUpdate, VacancySearch, ResponseUpdate
from uuid import UUID

async def get_vacancy_by_id(db: AsyncSession, vacancy_id: int) -> Optional[Vacancy]:
    result = await db.execute(select(Vacancy).filter(Vacancy.id == vacancy_id))
    return result.scalar_one_or_none()

async def get_vacancies_by_employer(db: AsyncSession, employer_id: UUID) -> List[Vacancy]:
    result = await db.execute(select(Vacancy).where(Vacancy.employer_id == employer_id))
    return list(result.scalars().all())

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

async def toggle_vacancy_active(db: AsyncSession, vacancy_id: int) -> bool:
    db_vacancy = await get_vacancy_by_id(db, vacancy_id)
    if not db_vacancy:
        return False
    
    db_vacancy.is_active = not db_vacancy.is_active
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
    return list(result.scalars().all())

async def get_responses_by_applicant(db: AsyncSession, applicant_id: UUID) -> List[Response]:
    result = await db.execute(
        select(Response)
        .options(joinedload(Response.vacancy))
        .where(Response.applicant_id == applicant_id)
        .order_by(desc(Response.created_at))
    )
    return list(result.scalars().all())

async def update_response_status(db: AsyncSession, response_id: int, update_data: ResponseUpdate) -> Optional[Response]:
    response = await get_response_by_id(db, response_id)
    if not response:
        return None
    
    response.status = update_data.status
    await db.commit()
    await db.refresh(response)
    return response

async def search_vacancies(db: AsyncSession, search_params: VacancySearch, page: int = 1, limit: int = 20) -> List[Vacancy]:
    
    query = select(Vacancy).where(Vacancy.is_active == True)
    query = query.options(joinedload(Vacancy.employer))
    
    if search_params.search_text:
        search_text = f"%{search_params.search_text}%"
        query = query.where(or_(Vacancy.title.ilike(search_text),
                Vacancy.description.ilike(search_text),
                Vacancy.tags.contains([search_params.search_text])))
    
    if search_params.tags:
        for tag in search_params.tags:
            query = query.where(Vacancy.tags.contains([tag]))
    
    if search_params.location:
        query = query.where(Vacancy.location.ilike(f"%{search_params.location}%"))
    
    if search_params.is_remote is not None:
        query = query.where(Vacancy.is_remote == search_params.is_remote)
    
    if search_params.min_salary is not None:
        query = query.where(or_(Vacancy.salary_from >= search_params.min_salary,
                Vacancy.salary_to >= search_params.min_salary))
    
    if search_params.max_salary is not None:
        query = query.where(or_(Vacancy.salary_from <= search_params.max_salary,
                Vacancy.salary_to <= search_params.max_salary))
    
    if search_params.salary_currency:
        query = query.where(Vacancy.salary_currency == search_params.salary_currency)
    
    if search_params.created_after:
        query = query.where(Vacancy.created_at >= search_params.created_after)
    
    if search_params.created_before:
        query = query.where(Vacancy.created_at <= search_params.created_before)
    
    if search_params.sort_by:
        sort_field, sort_order = search_params.sort_by.split("_")
        order_func = desc if sort_order == "desc" else asc
        
        if sort_field == "salary":
            avg_salary = (Vacancy.salary_from + Vacancy.salary_to) / 2
            query = query.order_by(order_func(avg_salary))
        elif sort_field == "date":
            query = query.order_by(order_func(Vacancy.created_at))
        elif sort_field == "title":
            query = query.order_by(order_func(Vacancy.title))
    else:
        query = query.order_by(desc(Vacancy.created_at))
    
    offset = (page - 1) * limit
    query = query.offset(offset).limit(limit)
    result = await db.execute(query)
    vacancies = result.scalars().unique().all()
    
    return list(vacancies)