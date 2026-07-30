# ALE Advanced Control Studio

## Default Behavior

This file defines the default behavior expected from AI agents working in this repository. Agents should follow the documented architecture, preserve existing patterns, and keep the solution maintainable and consistent.

## Project Purpose

ALE Advanced Control Studio (ALE ACS) is an Advanced Control Engineering platform for designing, configuring, simulating, and managing advanced industrial control strategies. The project is intended to become a complete industrial engineering environment for control engineers, automation engineers, and related technical users.

## Architecture

The repository follows a layered Clean Architecture approach with clear separation between presentation, application, domain, infrastructure, engine, export, and shared concerns. The Engineering Engine is the computational core of ALE ACS and should host advanced control algorithms and engineering capabilities. Business logic should never be placed in the Presentation layer; UI concerns remain separate from domain and engine behavior. The architecture also emphasizes MVVM in the presentation layer, dependency injection through the host model, and maintainable project boundaries.

## Architecture First

Architectural consistency has priority over implementation speed. If a requested implementation conflicts with the documented architecture, preserve the architecture and clearly identify the conflict before proceeding.

## Layer Responsibilities

- Presentation: hosts WPF views, view models, dialogs, and UI-related services.
- Application: coordinates use cases, application services, and workflows.
- Domain: contains core business concepts, entities, interfaces, and domain rules.
- Engine: implements computational control algorithms and engineering capabilities.
- Infrastructure: provides concrete implementations for persistence, configuration, logging, serialization, and other technical concerns.
- Export: handles structured export and reporting capabilities.
- Shared: provides reusable cross-cutting utilities and common types.

## Before Making Changes

- Read the affected project.
- Understand the architecture.
- Reuse existing patterns.
- Avoid duplicate implementations.
- Keep changes as small as possible.

## AI Agent Responsibilities

AI agents working in this repository should:

- Understand the task in the context of the repository’s architecture and documented goals.
- Keep changes aligned with the existing solution structure and the intended layer boundaries.
- Preserve the engineering-first approach and support maintainability, testability, and documentation.
- Implement the requested work using the smallest appropriate change set.
- Follow the existing development workflow and update documentation when the change affects architecture, behavior, or project scope.

## AI Agent Restrictions

AI agents must not:

- Modify the architecture without explicit request.
- Remove tests to make builds pass.
- Introduce breaking changes intentionally.
- Change files unrelated to the requested task.
- Make changes larger than necessary to complete the task.
- Do not move files unless explicitly requested.
- Do not rename public APIs unnecessarily.
- Do not introduce new dependencies without justification.
- Do not create duplicate abstractions.
- Do not replace existing patterns with personal preference.
- Ignore coding standards, repository organization, or documented workflow requirements.
- Break the intended separation of concerns between projects.

## Development Workflow

Understand Task

↓

Plan

↓

Implement

↓

Build

↓

Test

↓

Review

↓

Commit

## Definition of Done

A task is considered complete only if:

- Build succeeds.
- Tests succeed.
- Architecture remains consistent.
- Layer boundaries are respected.
- Existing functionality is preserved.
- No unrelated files were modified.
- Only the requested scope was modified.
- Documentation is updated when necessary.
