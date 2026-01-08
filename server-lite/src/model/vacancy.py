from sqlalchemy import Column, Integer, String, Text, Boolean, DateTime, ForeignKey, Enum
from sqlalchemy.sql import func
from sqlalchemy.orm import relationship
from sqlalchemy.dialects.postgresql import UUID, ARRAY
from database import Base
import enum

class Vacancy(Base):
    __tablename__ = "vacancies"
    
    id = Column(Integer, primary_key=True, index=True)
    title = Column(String(200), nullable=False)
    description = Column(Text, nullable=False)
    tags = Column(ARRAY(String))
    salary_from = Column(Integer, nullable=False)
    salary_to = Column(Integer, nullable=False)
    salary_currency = Column(String(3), default="RUB", nullable=False)
    location = Column(String(100), nullable=False)
    is_remote = Column(Boolean, default=False)
    is_active = Column(Boolean, default=True)
    
    employer_id = Column(UUID, ForeignKey("users.id"), nullable=False)
    
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())
    
    employer = relationship("User", back_populates="vacancies")
    responses = relationship("Response", back_populates="vacancy")

class ResponseStatus(str, enum.Enum):
    PENDING = "pending"
    REJECTED = "rejected"
    ACCEPTED = "accepted"

class Response(Base):
    __tablename__ = "responses"
    
    id = Column(Integer, primary_key=True, index=True)
    status = Column(Enum(ResponseStatus), default=ResponseStatus.PENDING)
    
    vacancy_id = Column(Integer, ForeignKey("vacancies.id"), nullable=False)
    applicant_id = Column(UUID, ForeignKey("users.id"), nullable=False)
    
    created_at = Column(DateTime(timezone=True), server_default=func.now())
    updated_at = Column(DateTime(timezone=True), onupdate=func.now())
    
    vacancy = relationship("Vacancy", back_populates="responses")
    applicant = relationship("User", back_populates="responses")