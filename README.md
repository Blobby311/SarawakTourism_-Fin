# Sarawak Tourism API Documentation

## Base URL
```
# Local Development
https://localhost:7151

# Production (Azure)
http://sarawaktourismapp-cadscsddbphmb7hx.southeastasia-01.azurewebsites.net
```

## Authentication

### Register a New User
```http
POST /api/auth/register
Content-Type: application/json

{
    "email": "user@example.com",
    "username": "username",
    "password": "password123"
}
```

Response:
```json
{
    "status": 200,
    "message": "Registration successful",
    "data": {
        "id": 1,
        "email": "user@example.com",
        "username": "username"
    }
}
```

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
    "email": "user@example.com",
    "password": "password123"
}
```

Response:
```json
{
    "status": 200,
    "message": "Success",
    "data": {
        "id": 1,
        "email": "user@example.com",
        "username": "username"
    }
}
```

## Forum Posts

### Get All Posts
```http
GET /api/posts
```

Response:
```json
{
    "status": 200,
    "data": [
        {
            "id": 1,
            "title": "Exploring Mulu Caves",
            "content": "The caves were amazing!",
            "imageUrl": "https://example.com/image.jpg",
            "userId": 1,
            "createdAt": "2024-05-02T12:00:00Z",
            "updatedAt": "2024-05-02T12:00:00Z",
            "user": {
                "id": 1,
                "username": "username"
            }
        }
    ]
}
```

### Create a New Post
```http
POST /api/posts
Content-Type: application/json

{
    "title": "My Trip to Bako National Park",
    "content": "Saw proboscis monkeys!",
    "imageUrl": "https://example.com/bako.jpg",
    "userId": 1
}
```

Response:
```json
{
    "status": 200,
    "data": {
        "id": 2,
        "title": "My Trip to Bako National Park",
        "content": "Saw proboscis monkeys!",
        "imageUrl": "https://example.com/bako.jpg",
        "userId": 1,
        "createdAt": "2024-05-02T12:30:00Z",
        "updatedAt": "2024-05-02T12:30:00Z"
    }
}
```

## Comments

### Get Comments for a Post
```http
GET /api/comments/post/1
```

Response:
```json
{
    "status": 200,
    "data": [
        {
            "id": 1,
            "content": "How was the weather?",
            "postId": 1,
            "userId": 2,
            "createdAt": "2024-05-02T13:00:00Z",
            "updatedAt": "2024-05-02T13:00:00Z",
            "user": {
                "id": 2,
                "username": "anotheruser"
            }
        }
    ]
}
```

### Add a Comment
```http
POST /api/comments
Content-Type: application/json

{
    "content": "Great post! I want to visit too.",
    "postId": 1,
    "userId": 2
}
```

Response:
```json
{
    "status": 200,
    "data": {
        "id": 2,
        "content": "Great post! I want to visit too.",
        "postId": 1,
        "userId": 2,
        "createdAt": "2024-05-02T13:30:00Z",
        "updatedAt": "2024-05-02T13:30:00Z"
    }
}
```

## Error Responses

All endpoints may return the following error format:

```json
{
    "status": 400,
    "message": "Error message here"
}
```

Common status codes:
- 200: Success
- 400: Bad Request (invalid data)
- 401: Unauthorized (not logged in)
- 404: Not Found
- 500: Server Error

## Notes for Frontend Team

1. **Authentication**:
   - Store the user ID after login for creating posts/comments
   - Include user ID in post/comment creation requests

2. **Posts**:
   - Posts can include images (optional)
   - Each post is linked to a user
   - Posts support likes functionality

3. **Comments**:
   - Comments are linked to both posts and users
   - Can be retrieved for specific posts

4. **Notifications**:
   - Users receive notifications for post likes and comments
   - Notifications can be marked as read

## Development Setup

1. Clone the repository
2. Run the backend:
   ```bash
   dotnet run
   ```
3. The API will be available at `https://localhost:7151`
4. Use the endpoints as documented above

## Deployment

### Azure Deployment
1. Right-click on the project in Visual Studio
2. Select "Publish"
3. Choose "Azure" as the target
4. Select "Azure App Service (Windows)"
5. Create a new App Service or select an existing one
6. Configure the following settings:
   - Name: sarawaktourismapi (or your preferred name)
   - Subscription: Azure for Students
   - Resource Group: SarawakTourism
   - Hosting Plan: SarawakTourismPlan (Windows, Free F1)
   - Region: Southeast Asia
7. Click "Create" and then "Publish"

### Database Considerations
- The application uses SQLite locally
- For production deployment, consider migrating to Azure SQL Database
- Update the connection string in `appsettings.json` for production

### Environment Variables
Make sure to set the following in Azure App Service Configuration:
- ASPNETCORE_ENVIRONMENT: Production
- ConnectionStrings__DefaultConnection: Your production database connection string 