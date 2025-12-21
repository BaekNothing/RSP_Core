# Build & Target Framework

Source: https://baeknothing.atlassian.net/wiki/spaces/RSPCore/pages/86737348/Build+Target+Framework

1. 목적과 지위

본 문서는 RSP_Core의 빌드 대상, 실행 환경, 기술적 제약을 정의한다.
이 문서는 단순한 개발 환경 가이드가 아니라,
RSP_Core Architecture가 유효하게 유지되기 위한 전제 조건을 명시한다.

여기서 정의된 제약은 선택 사항이 아니라,
Architecture의 일관성과 자동화 가능성을 보장하기 위한 강제 조건으로 취급된다.

2. Target Framework

2.1 기본 타겟

- 
Target Framework: .NET Standard 2.1


이 선택은 다음 요구를 동시에 만족시키기 위한 것이다.

- 
Unity 6.x 환경과의 호환성


- 
순수 C# 테스트 환경에서의 실행 가능성


- 
플랫폼 독립적인 DLL 배포


2.2 타겟 선택의 의미

.NET Standard 2.1은 다음을 보장한다.

- 
런타임 환경에 종속되지 않는 API 표면


- 
CI 환경에서의 동일한 실행 결과


- 
Host(Unity, Console, Test) 간 코드 공유


Target Framework 변경은
광범위한 영향 범위를 가지는 Architecture 변경으로 취급된다.

3. Unity 의존성 정책

3.1 UnityEngine 참조 금지

RSP_Core는 다음을 원칙적으로 금지한다.

- 
UnityEngine 네임스페이스 참조


- 
MonoBehaviour 기반 구조


- 
Unity 전용 타입(Vector3, GameObject 등) 사용


이는 다음을 보장하기 위한 제약이다.

- 
Headless 실행


- 
테스트 환경 독립성


- 
장기적 엔진 재사용성


이 정책의 완화 또는 예외 도입은
Architecture 변경으로 간주된다.

3.2 Host와의 경계

Unity 관련 로직은 전부 Host 책임이다.

- 
입력 처리


- 
UI / 연출


- 
프레임 기반 업데이트


- 
에셋 로딩


RSP_Core는 프레임 개념을 알지 못한다.

4. Reflection / Dynamic 사용 정책

4.1 기본 원칙

- 
Reflection 사용은 최소화


- 
dynamic 키워드 사용은 원칙적으로 지양


이 제약은 성능보다는 다음을 우선한다.

- 
테스트 가능성


- 
코드 분석 가능성


- 
자동 Spec / Migration 생성의 정확도


4.2 허용 범위

Reflection 사용이 불가피한 경우에도 다음 조건을 만족해야 한다.

- 
Core 로직이 아닌 테스트 보조 또는 도구 코드


- 
실행 경로에 영향이 없는 영역


- 
명시적으로 문서화된 경우


Core 전투 로직에 Reflection이 도입되는 경우,
이는 고위험 Architecture 변경으로 취급된다.

5. 테스트 가능성 우선 원칙

5.1 Headless Test 지원

RSP_Core는 다음 테스트 형태를 전제로 설계된다.

- 
순수 C# 단위 테스트


- 
Unity Editor 외부 실행


- 
CI 환경에서의 반복 실행


5.2 결정적 테스트

- 
동일 입력 → 동일 결과


- 
난수는 반드시 주입 가능해야 함


- 
시간, 프레임, 외부 상태에 의존하지 않음


결정성 훼손은 Behavior change로 취급된다.

5.3 Snapshot 기반 검증

- 
테스트는 Snapshot 비교를 기준으로 수행 가능해야 한다


- 
내부 상태 직접 접근은 필요하지 않아야 한다


6. 빌드 산출물 정책

6.1 산출물 형태

- 
단일 DLL (RSP_Core.dll)


- 
플랫폼 독립적 빌드 결과


6.2 배포 대상

- 
Unity 프로젝트의 Plugins 폴더


- 
테스트 프로젝트 참조


- 
콘솔/툴링 환경


빌드 산출물 구조 변경은
구현 변경으로 취급되며,
Architecture 변경은 아니다(의미 변화가 없는 한).

7. CI / 자동화 관점

본 문서의 제약은 다음 자동화를 전제로 한다.

- 
CI 환경에서의 빌드


- 
자동 테스트 실행


- 
Architecture 변경 diff 기반 Spec / Migration 생성


특히 다음 조건이 깨질 경우 자동화는 신뢰할 수 없게 된다.

- 
런타임 환경 의존성 증가


- 
비결정적 실행 경로 도입


- 
테스트 불가능한 코드 경로 추가


8. 변경에 대한 판단 기준

변경 유형

판단

Target Framework 변경

Architecture 변경

UnityEngine 참조 도입

Architecture 변경

Core 로직 Reflection 도입

고위험 Architecture 변경

테스트 보조용 도구 추가

Extension

빌드 스크립트 개선

Clarification / Implementation

9. 요약

Build & Target Framework 문서는
RSP_Core가 어디에서, 어떤 조건으로, 어떻게 실행될 수 있는지를 정의한다.

여기서 정의된 제약은 구현 편의가 아니라,
Architecture의 지속 가능성과 자동화 신뢰성을 위한 선택이다.

본 문서는 이후 모든 구현·테스트·자동화 판단의
**환경적 기준선(baseline)**으로 기능한다.
