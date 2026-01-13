from pydantic import BaseModel, Field, field_validator
from typing import Optional, List
from uuid import UUID
from datetime import datetime
import re

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

class VacancySearch(BaseModel):
    search_text: Optional[str] = None
    
    tags: Optional[List[str]] = None
    location: Optional[str] = None
    is_remote: Optional[bool] = None
    
    min_salary: Optional[int] = None
    max_salary: Optional[int] = None
    salary_currency: Optional[str] = None
    
    created_after: Optional[datetime] = None
    created_before: Optional[datetime] = None
    
    sort_by: Optional[str] = Field(default=None, description="Sort: salary_desc, date_asc, title_asc")

    @field_validator('sort_by')
    @classmethod
    def validate_sort_by(cls, v):
        if v is None:
            return v
        
        pattern = r'^(salary|date|title)_(asc|desc)$'
        if not re.match(pattern, v):
            raise ValueError(
                'sort_by must be: field_order (salary_desc, date_asc, title_asc)'
            )
        return v
    
    class Config:
        from_attributes = True