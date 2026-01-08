from pydantic import BaseModel, Field
from typing import Optional, List
from uuid import UUID

class VacancyBase(BaseModel):
    title: str
    description: str
    tags: List[str] = []
    salary_from: int
    salary_to: int
    salary_currency: str = "RUB"
    location: Optional[str] = None
    is_remote: bool = False

class VacancyCreate(VacancyBase):
    pass

class VacancyUpdate(BaseModel):
    title: Optional[str] = None
    description: Optional[str] = None
    tags: Optional[List[str]] = None
    salary_from: Optional[int] = None
    salary_to: Optional[int] = None
    salary_currency: Optional[str] = None
    location: Optional[str] = None
    is_remote: Optional[bool] = None

class VacancyResponse(VacancyBase):
    id: int
    employer_id: UUID
    is_active: bool

    class Config:
        from_attributes = True