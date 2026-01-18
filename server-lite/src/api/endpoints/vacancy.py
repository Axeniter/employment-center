from fastapi import APIRouter, Depends, HTTPException, status
from schemas.vacancy import VacancyResponse, VacancyCreate, VacancyUpdate, VacancySearch
from orm.vacancy import create_vacancy, update_vacancy, get_vacancy_by_id, delete_vacancy, toggle_vacancy_active
from core.database import get_db
from core.dependencies import require_role
from models.user import UserRole
from sqlalchemy.ext.asyncio import AsyncSession

vacancy_router = APIRouter(prefix="/vacancies", tags=["vacancy"])

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
