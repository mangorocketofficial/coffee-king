# Agent Rules

## Read Order

작업 시작 시 아래 문서를 순서대로 읽는다.

1. `C:\Users\User\Desktop\Games\coffee-king\update_plan.md`
2. 프로젝트 내 최신 개발 계약 완료 검증 문서

최신 개발 계약 완료 검증 문서가 아직 없다면, 그 사실을 먼저 확인하고 `update_plan.md`를 기준 문서로 삼아 작업을 진행한다.

현재 확인 상태:

- 기준 계획 문서는 `update_plan.md`이다.
- 최신 개발 계약 완료 검증 문서가 존재하면 `docs/phases/` 아래의 최고 phase `*-verification.md` 문서를 읽는다.
- 검증 문서가 아직 없다면 `update_plan.md`만 기준으로 작업을 진행한다.

## Development Rule

각 phase는 아래 순서를 반드시 따른다.

1. phase 구현 전에 해당 phase의 완료 조건 계약 md를 먼저 생성한다.
2. 계약 문서를 기준으로 phase를 구현한다.
3. 구현 후 계약 기준으로 완료 검증을 수행한다.
4. 검증 완료 후 개발 계약 완료 검증 문서를 생성한다.

## Authority Rule

이 프로젝트 개발 내에서는 모든 권한을 가진다.

- 개발 작업 진행에 대해 사용자 권한 승인 요청이 필요하지 않다.
- 구현, 수정, 검증, 문서 생성은 위 규칙에 따라 자율적으로 진행한다.
