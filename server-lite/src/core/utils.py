import os
from uuid import UUID
from fastapi import UploadFile, HTTPException
from config import config

async def save_user_avatar(file: UploadFile, user_id: UUID):
    try:
        if not file.filename:
            raise HTTPException(
                status_code=400,
                detail="No filename provided"
            )
        
        file.file.seek(0, 2)
        file_size = file.file.tell()
        file.file.seek(0)
        
        if file_size > config.MAX_FILE_SIZE:
            raise HTTPException(status_code=400, detail=f"File too large")
        
        file_extension = os.path.splitext(file.filename)[1].lower()
        
        if file_extension not in config.ALLOWED_EXTENSIONS:
            raise HTTPException(status_code=400, detail=f"File type not allowed")

        for extension in config.ALLOWED_EXTENSIONS:
            old_filename = f"{user_id}{extension}"
            old_file_path = os.path.join(config.UPLOAD_DIR, old_filename)
            
            if os.path.exists(old_file_path):
                os.remove(old_file_path)

        filename = f"{user_id}{file_extension}"
        
        os.makedirs(config.UPLOAD_DIR, exist_ok=True)
        
        file_path = os.path.join(config.UPLOAD_DIR, filename)
        
        with open(file_path, "wb") as buffer:
            content = await file.read()
            buffer.write(content)
            
        return filename
        
    except HTTPException:
        raise
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error saving file: {str(e)}")

def delete_user_avatar(user_id: UUID):
    try:
        avatar_deleted = False
        for extension in config.ALLOWED_EXTENSIONS:
            filename = f"{user_id}{extension}"
            file_path = os.path.join(config.UPLOAD_DIR, filename)
            
            if os.path.exists(file_path):
                os.remove(file_path)
                avatar_deleted = True

        return avatar_deleted
    
    except Exception:
        return False
    
def get_user_avatar_path(user_id: UUID):
    for extension in config.ALLOWED_EXTENSIONS:
        filename = f"{user_id}{extension}"
        file_path = os.path.join(config.UPLOAD_DIR, filename)
        
        if os.path.exists(file_path):
            return file_path
    
    return None