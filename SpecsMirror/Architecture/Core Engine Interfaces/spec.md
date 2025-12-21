# Core Engine Interfaces

Source: https://baeknothing.atlassian.net/wiki/spaces/RSPCore/pages/86900822/Core+Engine+Interfaces

1. 목적과 지위

본 문서는 **RSP_Core의 외부 인터페이스(Core Engine Interfaces)**와
그 **의미적 계약(semantic contract)**을 정의한다.

Core Engine Interfaces는 다음 성격을 가진다.

- 
RSP_Core의 유일한 외부 진입점


- 
Host와 Engine 간 책임 경계의 최전선


- 
Architecture 변경 여부를 판단하는 1차 기준


본 문서에서 정의된 인터페이스의 의미 변화는
코드 변경 이전에 Spec 및 Migration 생성 대상으로 취급된다.

2. 인터페이스 설계 원칙

2.1 단일 진입점 원칙

- 
Host는 반드시 ICombatEngine을 통해서만 Engine에 접근한다.


- 
내부 구현 클래스에 대한 직접 접근은 허용하지 않는다.


이는 테스트, 자동화, 안정성 확보를 위한 구조적 선택이다.

2.2 명시적 상태 전이

- 
모든 상태 변화는 의도가 명시된 메서드 호출로만 발생한다.


- 
암묵적 상태 변화(자동 턴 진행, 내부 타이머 등)는 허용하지 않는다.


2.3 호출 순서 불변식

메서드는 의미적으로 유효한 호출 순서를 가진다.
이 순서 위반은 런타임 예외로 처리된다.

2.4 Snapshot 중심 외부 노출

- 
외부 시스템은 내부 상태를 직접 참조하거나 수정하지 않는다.


- 
모든 상태 관측은 Snapshot을 통해 이루어진다.


3. ICombatEngine 개요

ICombatEngine은 RSP_Core의 유일한 외부 제어 인터페이스다.

제공 메서드

- 
Initialize


- 
ResolveCard


- 
NextTurn


- 
GetSnapshot


이 네 메서드 외의 제어 수단은 존재하지 않는다.

4. 메서드별 계약 정의

4.1 Initialize

의미

- 
전투 세션을 시작하기 위한 초기 상태를 주입한다.


- 
내부 BattleState를 구성한다.


계약

- 
Initialize는 한 전투 세션당 1회만 호출 가능


- 
초기 상태는 Host가 구성한 값을 그대로 수용한다


- 
호출 이후 Engine은 “Initialized” 상태로 진입한다


호출 순서 제약

- 
다른 모든 메서드보다 반드시 선행


안정성 레벨

- 
High (Stable)


의미 변경 시 Migration 필수.

4.2 ResolveCard

의미

- 
플레이어가 선택한 카드 1장을 해결(resolve)한다.


- 
단일 슬롯 단위의 전투 판정을 수행한다.


계약

- 
CardInstanceId는 반드시 PlayerState.Hand에 존재해야 한다


- 
에너지가 부족한 경우:

- 
상태 변화 없음


- 
EventTags에 EnergyInsufficient 포함



- 
카드 해결 결과는 Result + Snapshot으로 반환된다


호출 순서 제약

- 
Initialize 이후에만 호출 가능


- 
동일 턴 내에서 여러 번 호출 가능


- 
Turn 종료는 ResolveCard가 아니라 NextTurn으로만 수행된다


안정성 레벨

- 
High (Stable)


의미 변경은 Behavior change로 취급된다.

4.3 NextTurn

의미

- 
현재 턴을 종료하고 다음 턴으로 전이한다.


- 
턴 경계에서 수행되는 초기화를 담당한다.


계약

- 
에너지 리필


- 
방어도 초기화


- 
카드 드로우


- 
턴 번호 증가


구체 정책은 Architecture / Data Contracts에 정의된 범위를 따른다.

호출 순서 제약

- 
Initialize 이후에만 호출 가능


- 
ResolveCard 호출 없이도 호출 가능


- 
연속 호출은 의미를 가지지 않는다


안정성 레벨

- 
High (Stable)


턴 경계 의미 변경은 Migration 필수.

4.4 GetSnapshot

의미

- 
현재 전투 상태를 외부 관측용으로 반환한다.


계약

- 
Snapshot은 읽기 전용 개념


- 
내부 상태와 참조를 공유하지 않는다


- 
동일 시점에서는 항상 동일한 값을 반환한다


호출 순서 제약

- 
Initialize 이후에만 호출 가능


- 
다른 메서드 호출 사이에 자유롭게 호출 가능


안정성 레벨

- 
Very High (Highly Stable)


Snapshot 의미 변경은 최고 위험 변경으로 취급된다.

5. 예외 및 오류 보고 정책

5.1 예외(Exception)

다음 상황은 예외로 처리된다.

- 
Initialize 이전 메서드 호출


- 
Hand에 존재하지 않는 CardInstanceId


- 
잘못된 초기화 입력(null 등)


예외는 프로그래밍 오류를 나타낸다.

5.2 EventTags

다음 상황은 EventTags로 보고된다.

- 
에너지 부족


- 
Effect 누락


- 
Effect 실행 오류


EventTags는 비치명적 오류 및 분기 힌트다.

EventTags 의미 변경은 Behavior change로 간주된다.

6. 인터페이스 안정성 요약

메서드

안정성 레벨

변경 판단

Initialize

High

Migration 필수

ResolveCard

High

Migration 필수

NextTurn

High

Migration 필수

GetSnapshot

Very High

최고 위험

7. 자동화 에이전트 관점 요약

Core Engine Interfaces 변경 시 자동화 에이전트는 다음을 수행한다.

- 
시그니처 변경 → Breaking change


- 
의미 변경 → Spec + Migration 생성


- 
문서 보강 → Clarification


이 판단은 본 문서를 기준으로 수행된다.

8. 기술적 제약

Core Engine Interfaces는 다음 제약을 따른다.

- 
.NET Standard 2.1


- 
UnityEngine 참조 없음


- 
Reflection / Dynamic 최소화


- 
테스트 코드에서 직접 호출 가능해야 함


이 제약은 결정적 실행과 자동 검증을 전제로 한다.

9. 요약

Core Engine Interfaces는 RSP_Core Architecture의 외부 경계면이다.
여기서 정의된 의미가 바뀌는 순간,
그 변화는 코드 이전에 Spec과 Migration으로 먼저 드러나야 한다.

본 문서는 그 판단을 가능하게 하는 **기준선(baseline)**으로 기능한다.
