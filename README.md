# EV Charging Station ERP System ⚡🚗

## Overview

This project is an **end-to-end Enterprise Resource Planning (ERP) system for an EV Charging Station platform**, developed collaboratively by **4 developers**.  
The system supports **booking management, operations, and user interactions** across multiple roles and platforms, following **enterprise-grade architecture and best practices**.

The solution consists of:
- A **role-based web application**
- An **offline-first Android mobile application**
- A **centralized backend with NoSQL data storage**
- **Enterprise deployment using IIS**

---

## System Components

### 1. Web Application (ERP Portal)

The web application is designed for operational and administrative users with **role-based access control**.

**User Roles**
- **BackOffice Admin**
  - System configuration
  - User and role management
  - Station and charger management
- **Operators**
  - Booking approvals
  - Charging session monitoring
  - Operational reporting
- **EV Owners**
  - Booking charging slots
  - Viewing charging history
  - Managing profiles

**Key Features**
- Secure authentication and authorization
- Role-based UI rendering
- Booking and scheduling system
- Centralized management dashboard
- REST API integration with backend services

---

### 2. Android Mobile Application (Offline-First)

The Android application is built specifically for **reliability in low or unstable network environments**.

**Key Characteristics**
- **Offline-first architecture**
- Local data storage using **SQLite**
- Seamless user experience without constant internet access

**Core Features**
- View and manage bookings
- Access charging station data offline
- Local caching of user and booking data

---

### 3. Database Synchronization Manager

A dedicated **DB Synchronization Manager** ensures data consistency between the mobile app and backend.

**Responsibilities**
- Sync local SQLite database with the central **MongoDB**
- Conflict handling and resolution
- Retry mechanisms for failed sync attempts
- Efficient incremental updates

This enables:
- High availability
- Reduced data loss
- Enterprise-level reliability

---

### 4. Backend Services

**Architecture**
- RESTful API-based backend
- Stateless services
- Centralized business logic

**Database**
- **MongoDB (NoSQL)** as the primary database
- Flexible schema for scalability
- Optimized for high read/write operations

**Security**
- JWT-based authentication
- Role-based authorization
- Secure API endpoints

---

### 5. Deployment & Server Infrastructure

The backend services are deployed using **Microsoft IIS (Internet Information Services)**.

**Why IIS**
- Enterprise-grade Windows server support
- Seamless integration with .NET applications
- Process management and application pooling
- Secure and scalable hosting

**Deployment Highlights**
- Hosted on IIS without requiring manual runtime execution
- Application Pool configured with:
  - Integrated pipeline mode
  - Proper permission management
- Production-ready deployment setup

---

## Enterprise-Level Practices Used

### 🔐 Security Best Practices
- JWT-based authentication
- Role-based access control (RBAC)
- Secure API design
- Environment-based configuration

### 🏗 Architecture & Design
- Separation of concerns (Frontend, Backend, Mobile)
- RESTful API architecture
- Offline-first mobile design
- Centralized business logic

### 📦 Data Management
- NoSQL database design (MongoDB)
- Local caching with SQLite
- Synchronization and conflict resolution strategies

### 🚀 Deployment & Operations
- IIS-based production deployment
- No need for manual server execution (`dotnet run`)
- Controlled publishing and versioning
- Scalable server configuration

### 👥 Collaboration & Development
- Team-based development with version control
- Clear responsibility separation
- Modular and maintainable codebase
- Enterprise-ready documentation

---

## Technologies Used

**Frontend**
- Web technologies with role-based UI
- REST API integration

**Backend**
- .NET-based backend services
- MongoDB (NoSQL)

**Mobile**
- Android
- SQLite (Local DB)
- Offline-first design

**Infrastructure**
- Microsoft IIS
- JWT Authentication
- RESTful APIs

---

## Key Skills Demonstrated

- Enterprise system design
- Full-stack development
- Offline-first mobile architecture
- Database synchronization
- NoSQL data modeling
- Secure authentication & authorization
- IIS server deployment
- Team collaboration on large-scale systems

---

## Conclusion

This project demonstrates the design and implementation of a **real-world, enterprise-scale ERP system** with:
- Multiple user roles
- Cross-platform support
- Offline capabilities
- Secure, scalable backend
- Professional deployment using IIS

It reflects strong **software engineering, system architecture, and DevOps fundamentals**, making it suitable for enterprise and production environments.

---

⚡ *Built as a collaborative enterprise solution for modern EV charging infrastructure.*
