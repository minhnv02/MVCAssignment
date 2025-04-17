# 📊 Assignment04-05-MVC — Unit Testing with NUnit

## 📖 Introduction

This project is an ASP.NET MVC application, developed with the goal of practicing **Unit Testing** using **NUnit** and ensuring a minimum **70% code coverage**.

---

## 🎯 Requirements

- Create Unit Tests for all actions in the `PersonRepository`
- Create Unit Tests for all business methods by `RookiesController`
- Achieve at least **70% code coverage** for both Controller and Service layers

---

## 🛠️ Implementation

### ✅ What has been done

- Applied **NUnit** for Unit Testing
- Wrote Unit Tests for all methods in:
  - `RookiesController`
  - `PersonRepository`
- Used **FluentValidation** for validating `Person` data
- Mocked data for:
  - Repository in `PersonService`
  - Service in `RookiesController`
- Utilized `[Test]` attributes of NUnit
- Applied **FluentAssertions** for asserting test results
- Followed **Arrange - Act - Assert** pattern for all test cases
- Created separate **TestData** for each method and controller

---

## 📂 Data Setup from CSV

To simulate and manage test data efficiently, we’ve **loaded test data from CSV files** for some scenarios:

- The CSV files contain mock data for `Person` entities.
- CSV files are located in the `TestData` folder within the `MVC.Tests` project.
- Data from these CSV files is read at runtime and converted into object collections using **CsvHelper**.

### 📑 Example

**TestData/Persons.csv**
