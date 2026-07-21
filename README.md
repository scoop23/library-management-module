# Library Management System — Book Management Module

This is the **Book Management** slice of the larger Library Management System spec:
Books, Book Copies, and Categories with full CRUD, search, filtering, and cover-image
upload. Barcode generation/printing is intentionally left out, and the other modules
(Authentication, Dashboard, Borrowing, Returns, Reservations, Fines, Reports) are
**not implemented here** — the architecture (repositories, services, Firestore
collections, folder layout) is built so those can be added later without rework.

## Stack

- .NET 8, Windows Forms (`net8.0-windows`)
- **Google.Cloud.Firestore** (official SDK) instead of raw REST/HttpClient
- **Google.Cloud.Storage.V1** for cover image uploads to Firebase Storage
- Three-layer architecture: Forms (Presentation) → Services (Business Logic) →
  Repositories (Data Access)

## Folder structure

```
/Forms            LoginForm-style UI: BooksForm, BookDetailsForm, BookCopiesForm, CategoriesForm
/Models           Book, BookCopy, Category (Firestore-mapped POCOs)
/Repositories     FirebaseRepository<T> generic base + Book/BookCopy/Category repos
/Services         BookService, BookCopyService, CategoryService, FirebaseStorageService
/Firebase         FirebaseConfig (Firestore bootstrap)
/Configurations   appsettings.json, service-account.example.json
firestore.rules   Sample security rules
```

## Prerequisites

- Visual Studio 2022 with the **.NET desktop development** workload
- .NET 8 SDK
- A Firebase project with **Firestore** and **Storage** enabled

## Setup

1. **Create/select a Firebase project** at https://console.firebase.google.com.
2. **Enable Firestore** (Native mode) and **Firebase Storage**.
3. **Generate a service account key**: Project Settings → Service Accounts →
   Generate new private key. Save the downloaded JSON as:
   `Configurations/service-account.json` (already git-ignored — never commit it).
4. Update `Configurations/appsettings.json` and `Program.cs` with your real
   `ProjectId` (and storage bucket, e.g. `your-project-id.appspot.com`).
5. Open `LibraryManagementSystem.sln` in Visual Studio 2022.
6. Restore NuGet packages (`Google.Cloud.Firestore`, `Google.Cloud.Storage.V1`)
   — happens automatically on build.
7. Set `LibraryManagementSystem` as the startup project and press **F5**.

## Firestore collections used

- `Books` — see `Models/Book.cs` for fields
- `BookCopies` — linked to a Book via `BookId`
- `Categories` — linked to a Book via `CategoryId`

Deploy the sample rules with:
```
firebase deploy --only firestore:rules
```
(after adjusting the custom-claim role check to match how you assign
Administrator/Librarian roles in Firebase Auth).

## What's implemented

- Book CRUD: add, edit, archive, delete (blocks delete while copies are
  borrowed/reserved), search by title, filter by category
- Book Copy CRUD per book, with automatic Total/Available copy count sync on
  the parent Book
- Category CRUD (needed for the category filter/picker)
- Optional cover image upload to Firebase Storage on save
