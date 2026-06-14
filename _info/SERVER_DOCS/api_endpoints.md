## API ENDPOINTS

**Authentication**
  POST /auth/login

**Projects**
  GET /projects
  GET /projects/{id}
  POST /projects
  PUT /projects/{id}
  DELETE /projects/{id}

**Drafts**
  GET /drafts
  POST /drafts
  POST /drafts/{draftId}/save
  DELETE /drafts/{id}

**Assignments**
  POST /assignments/assign-province
  GET /assignments/user/{userId}
  GET /assignments

**Expirations**
  GET /expirations
  GET /expirations/project/{projectId}
  POST /expirations/{expirationId}/extend
  POST /expirations/{expirationId}/reset
  POST /expirations/{expirationId}/handle-performance-bond
  POST /expirations/{expirationId}/handle-semestral-report

**Notifications**
  GET /notifications
  PUT /notifications/{id}/mark-read
  DELETE /notifications/{id}

**Users**
  GET /users/profile
  PUT /users/profile

**Municipalities**
  GET /municipalities
  GET /municipalities/{id}

**Project Types**
  GET /project-types

**Provinces**
  GET /provinces

**Central Project Network (CPN)**
  GET /cpn/projects
  GET /cpn/projects/{id}

**Real-time Events**
  WS /notificationHub (Note: Connected via WebSocket at /api/v1/notificationHub)