from core.database import Base
from sqlalchemy import Column, String, ForeignKey, DateTime, Text
from sqlalchemy.dialects.postgresql import UUID, ARRAY
from sqlalchemy.orm import relationship
from user import UserRole


class BaseProfile(Base):
    __abstract__ = True

    user_id = Column(UUID(as_uuid=True), ForeignKey("users.id"),
                     primary_key=True, index=True, ondelete="CASCADE")
    avatar_url = Column(String(500), nullable=True)

    @property
    def profile_type(self) -> UserRole:
        raise NotImplementedError
    
class ApplicantProfile(BaseProfile):
    __tablename__ = "applicant_profiles"

    first_name = Column(String(50), nullable=False)
    last_name = Column(String(50), nullable=False)
    middle_name = Column(String(50), nullable=True)
    phone_number = Column(String(20), nullable=True)
    birth_date = Column(DateTime, nullable=True)
    city = Column(String(50), nullable=True)
    about = Column(Text)
    skills = Column(ARRAY(String))

    user = relationship("User", back_populates="applicant_profile", uselist=False)

    responses = relationship("Response", back_populates="applicant")

    @property
    def profile_type(self) -> UserRole:
        return UserRole.APPLICANT
    
class EmployerProfile(BaseProfile):
    __tablename__ = "employer_profiles"

    company_name = Column(String(100), nullable=False)
    description = Column(Text)
    contact = Column(Text)

    user = relationship("User", back_populates="employer_profile", uselist=False)
    
    vacancies = relationship("Vacancy", back_populates="employer")
    events = relationship("Event", back_populates="employer")

    @property
    def profile_type(self) -> UserRole:
        return UserRole.EMPLOYER
