from fastapi import APIRouter, Depends, HTTPException, status, Query
from schemas.vacancy import VacancyResponse, VacancyCreate, VacancyUpdate, VacancySearch, ResponseResponse, ResponseCreate, ResponseUpdate
from orm.vacancy import (create_vacancy, update_vacancy, get_vacancy_by_id, delete_vacancy, toggle_vacancy_active, create_response,
                         get_responses_by_applicant, get_responses_by_vacancy, get_response_by_id, update_response_status,
                         get_vacancies_by_employer, search_vacancies)
from core.database import get_db
from core.dependencies import require_role, get_current_active_user
from models.user import UserRole
from sqlalchemy.ext.asyncio import AsyncSession
from typing import List

vacancy_router = APIRouter(prefix="/vacancies", tags=["vacancy"])
response_router = APIRouter(prefix="/responses", tags=["response"])

@vacancy_router.get("/me", response_model=List[VacancyResponse])
async def get_my_vacancies_endpoint(user = Depends(require_role(UserRole.EMPLOYER)), db: AsyncSession = Depends(get_db)):
    vacancies = await get_vacancies_by_employer(db, user.id)
    return vacancies

@vacancy_router.get("/", response_model=List[VacancyResponse])
async def search_vacancies_endpoint(search_params: VacancySearch = Depends(), page: int = Query(1, ge=1),
                                    limit: int = Query(20, ge=1, le=100), db: AsyncSession = Depends(get_db)):
    vacancies = await search_vacancies(db, search_params, page, limit)
    return vacancies

@vacancy_router.post("/", response_model=VacancyResponse)
async def create_vacancy_endpoint(vacancy_data: VacancyCreate, user = Depends(require_role(UserRole.EMPLOYER)),
                                  db: AsyncSession = Depends(get_db)):
    new_vacancy = await create_vacancy(db, vacancy_data, user.id)
    return new_vacancy

@vacancy_router.put("/{vacancy_id}", response_model=VacancyResponse)
async def update_vacancy_endpoint(vacancy_id: int, vacancy_data: VacancyUpdate, user = Depends(require_role(UserRole.EMPLOYER)),
                                  db: AsyncSession = Depends(get_db)):
    db_vacancy = await get_vacancy_by_id(db, vacancy_id)
    if not db_vacancy:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Vacancy not found"
        )
    
    if db_vacancy.employer_id != user.id:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="You can only update your own vacancies"
        )
    
    updated_vacancy = await update_vacancy(db, vacancy_id, vacancy_data)
    return updated_vacancy

@vacancy_router.delete("/{vacancy_id}")
async def delete_vacancy_endpoint(vacancy_id: int, user = Depends(require_role(UserRole.EMPLOYER)),
                                  db: AsyncSession = Depends(get_db)):
    db_vacancy = await get_vacancy_by_id(db, vacancy_id)
    if not db_vacancy:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Vacancy not found"
        )
    
    if db_vacancy.employer_id != user.id:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="You can only delete your own vacancies"
        )
    
    success = await delete_vacancy(db, vacancy_id)
    if not success:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail="Failed to delete vacancy"
        )
    
@vacancy_router.put("/{vacancy_id}/toggle", response_model=VacancyResponse)
async def toggle_vacancy_active_endpoint(vacancy_id: int, user = Depends(require_role(UserRole.EMPLOYER)),
                                         db: AsyncSession = Depends(get_db)):
    db_vacancy = await get_vacancy_by_id(db, vacancy_id)
    if not db_vacancy:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Vacancy not found"
        )
    
    if db_vacancy.employer_id != user.id:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="You can only modify your own vacancies"
        )
    
    modified_vacancy = await toggle_vacancy_active(db, vacancy_id)
    return modified_vacancy

@vacancy_router.get("/{vacancy_id}", response_model=VacancyResponse)
async def get_vacancy_by_id_endpoint(vacancy_id: int, db: AsyncSession = Depends(get_db)):
    db_vacancy = await get_vacancy_by_id(db, vacancy_id)
    if not db_vacancy:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Vacancy not found"
        )
    return db_vacancy

@vacancy_router.post("/{vacancy_id}/responses", response_model=ResponseResponse)
async def create_response_endpoint(vacancy_id: int, user = Depends(require_role(UserRole.APPLICANT)),
                                   db: AsyncSession = Depends(get_db)):
    db_vacancy = await get_vacancy_by_id(db, vacancy_id)

    if not db_vacancy:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Vacancy not found"
        )
    
    if not db_vacancy.is_active:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Cannot respond to inactive vacancy"
        )
    
    existing_responses = await get_responses_by_applicant(db, user.id)
    for response in existing_responses:
        if response.vacancy_id == vacancy_id:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="You have already responded to this vacancy"
            )
    
    response = await create_response(db, user.id, vacancy_id)
    return response

@vacancy_router.get("/{vacancy_id}/responses", response_model=List[ResponseResponse])
async def get_responses_by_vacancy_endpoint(vacancy_id: int, user = Depends(require_role(UserRole.EMPLOYER)),
                                            db: AsyncSession = Depends(get_db)):
    db_vacancy = await get_vacancy_by_id(db, vacancy_id)
    if not db_vacancy:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Vacancy not found"
        )
    
    if db_vacancy.employer_id != user.id or user.role != "employer":
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="Only the vacancy creator can view responses"
        )
    
    responses = await get_responses_by_vacancy(db, vacancy_id)
    return responses
    
@response_router.get("/{response_id}", response_model=ResponseResponse)
async def get_response_by_id_endpoint(response_id: int, user = Depends(get_current_active_user), db = Depends(get_db)):
    db_response = await get_response_by_id(db, response_id)

    if not db_response:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Response not found")
    
    if user.role == UserRole.APPLICANT:
        if db_response.applicant_id != user.id:
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail="You can only view your own responses")
    elif user.role == UserRole.EMPLOYER:
        vacancy = await get_vacancy_by_id(db, db_response.vacancy_id)
        if vacancy.employer_id != user.id:
            raise HTTPException(
                status_code=status.HTTP_403_FORBIDDEN,
                detail="You can only view responses to your vacancies")
        
    return db_response

@response_router.put("/{response_id}", response_model=ResponseResponse)
async def update_response_status_endpoint(response_id: int, status_update: ResponseUpdate, user = Depends(require_role(UserRole.EMPLOYER)),
                                          db: AsyncSession = Depends(get_db)):
    db_response = await get_response_by_id(db, response_id)
    if not db_response:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Response not found")
    
    vacancy = await get_vacancy_by_id(db, db_response.vacancy_id)
    if vacancy.employer_id != user.id:
        raise HTTPException(
            status_code=status.HTTP_403_FORBIDDEN,
            detail="You can only update responses to your vacancies")
    
    updated_response = await update_response_status(db, response_id, status_update)
    return updated_response


@response_router.get("/me", response_model=List[ResponseResponse])
async def get_my_responses(user = Depends(require_role(UserRole.APPLICANT)), db: AsyncSession = Depends(get_db)):
    responses = await get_responses_by_applicant(db, user.id)
    return responses
