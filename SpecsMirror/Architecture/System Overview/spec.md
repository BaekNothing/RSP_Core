# System Overview

Source: https://baeknothing.atlassian.net/wiki/spaces/RSPCore/pages/86900806/System+Overview

1. 목적과 위치

본 문서는 RSP_Core의 전체 시스템 구조와 책임 경계를 정의한다.
RSP_Core는 Unity 및 UI 계층으로부터 완전히 분리된 헤드리스(Core) 전투 엔진이며,
전투 규칙의 정확성, 재현성, 확장 가능성을 우선 목표로 한다.

본 문서는 다음을 명확히 하기 위해 작성된다.

- 
RSP_Core가 무엇을 담당하는지


- 
무엇을 의도적으로 담당하지 않는지


- 
어떤 변경이 Architecture 변경으로 취급되는지


이 문서는 Spec 및 Migration을 자동 생성하는 파이프라인의 상위 기준 문서로 기능한다.

2. 시스템 개요

RSP_Core는 다음과 같은 성격을 가진다.

- 
카드 기반 전투의 순수 로직 엔진


- 
UI, 연출, 입력, 저장과 완전 분리


- 
데이터 주입 기반 설계 (Card / Enemy / Effect)


- 
결정적(deterministic) 실행 가능


- 
Unity 6.x 환경에서 DLL 형태로 참조 가능


핵심 아이디어

전투는 상태(State)와 명령(Command)이 주어졌을 때,
결과(Result)로 변환되는 순수한 함수에 가깝다.

이 개념은 RSP_Core의 모든 설계 선택의 출발점으로 취급된다.

3. Architecture의 역할

Architecture 문서는 구현 세부가 아니라, 다음을 관리한다.

- 
전투 규칙의 불변식(invariants)


- 
상태 전이의 경계와 의미


- 
Engine과 Host 간의 책임 분리


- 
변경 시 파급 범위에 대한 기준


Architecture 변경은 자동화 에이전트의 입력으로 사용되며,
변경 유형에 따라 Spec, Migration, 코드 변경이 파생 생성된다.

4. 책임 경계

4.1 RSP_Core가 책임지는 영역

- 
전투 상태 관리 (Player / Enemy / Turn)


- 
카드 해결 규칙 (ResolveCard)


- 
심볼 상성 판정


- 
효과 실행 순서 및 결과 적용


- 
에러 검증 및 결과 보고 (Exception / EventTags)


4.2 RSP_Core가 책임지지 않는 영역

- 
UI, 애니메이션, 사운드


- 
입력 처리 (카드 선택 로직)


- 
게임 모드 고유 규칙 (슬롯 수 제한, 턴 제한 등)


- 
저장 / 불러오기


- 
네트워크 동기화


- 
AI 의사결정 및 밸런싱 로직


위 항목에 대한 요구사항 유입은
Architecture 외부 또는 별도 시스템의 책임으로 취급된다.

5. 고수준 구조

개념적으로 RSP_Core는 다음과 같은 흐름을 가진다.
wide760
- 
Host는 API 호출과 결과 소비만 담당한다.


- 
RSP_Core는 상태 변경과 규칙 적용만 담당한다.


- 
역방향 의존성은 허용하지 않는다.


6. 주요 내부 컴포넌트 (개념적)

본 섹션은 구현 세부가 아닌, 개념적 책임 분리를 설명한다.

ICombatEngine

- 
외부에서 접근 가능한 단일 진입점


- 
Initialize, ResolveCard, NextTurn 제공


BattleState

- 
전투 중 변경되는 모든 내부 상태의 집합


- 
외부에는 Snapshot 형태로만 노출


EffectRegistry

- 
effectId → 실행 함수 매핑


- 
내부 효과와 Host 주입 효과의 공존 지점


ValueCalculator

- 
수치 계산의 단일 책임 지점


- 
강화, 버프, 디버프 계산의 중앙화


SymbolResolver

- 
□ △ ○ 상성 판정 전담


- 
상성 규칙의 불변식 유지


이 구성의 변경은 대체로 Architecture 변경으로 취급된다.

7. 실행 흐름 요약

일반적인 전투 흐름은 다음과 같다.

- 
Host가 초기 상태를 구성하여 Initialize 호출


- 
Host가 카드 선택 후 ResolveCard 호출


- 
RSP_Core가:

- 
심볼 상성 판정


- 
효과 실행


- 
적 행동 처리



- 
결과 Snapshot 및 Result 반환


- 
Host가 NextTurn 호출


- 
전투 종료 시까지 반복


모든 상태 변화는 명시적인 API 호출을 통해서만 발생한다.

8. 설계 원칙

- 
단방향 의존성

- 
Host → RSP_Core



- 
명시적 상태 변화

- 
암묵적 상태 변경 금지



- 
확장 지점의 고립

- 
EffectRegistry를 통한 확장만 허용



- 
침묵 실패 금지

- 
모든 비정상 상황은 예외 또는 EventTag로 보고



9. 기술적 제약

RSP_Core는 다음 기술적 제약을 전제로 설계된다.

- 
Target Framework: .NET Standard 2.1


- 
UnityEngine 참조 금지


- 
Reflection / Dynamic 최소화


- 
테스트 가능성 우선


이 제약은 성능보다 예측 가능성과 검증 가능성을 우선하기 위한 선택이다.

10. 변경에 대한 기본 관점

Architecture 변경은 다음과 같이 해석된다.

- 
Rule change
전투 규칙이나 의미 변경
→ Spec + Migration + 코드 변경


- 
Extension
확장 지점 추가, 기존 의미 유지
→ Spec + 코드 변경


- 
Clarification
문서 명확화
→ 문서만 갱신


이 분류는 이후 자동화 파이프라인의 기준으로 사용된다.

11. 요약

System Overview는 RSP_Core가 제공하는 기능 목록이 아니라,
그 기능들이 어떤 규칙과 경계 안에서 생성·변경되는지를 정의한다.

본 문서는 이후 생성될 모든 Spec과 Migration의 상위 기준선으로 기능한다.
