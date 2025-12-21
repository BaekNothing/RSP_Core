# Data Contracts

Source: https://baeknothing.atlassian.net/wiki/spaces/RSPCore/pages/86835233/Data+Contracts

1. 목적 / Purpose

본 문서는 **RSP_Core의 외부 API 및 내부 엔진에서 사용하는 데이터 계약(Data Contracts)**을 정의한다.
Host(Unity / Console / Test)는 본 문서에 정의된 DTO 및 모델을 기준으로 RSP_Core와 상호작용한다.

이 문서는 구현 세부가 아니라 다음을 중심으로 작성된다.

- 
데이터 구조의 형식(structure)


- 
각 필드의 의미(semantics)


- 
시스템 차원에서 유지되어야 할 불변 조건(invariants)


데이터 계약의 변경은 코드 변경 이전에 감지되어야 하며,
의미적 변경은 Migration 문서(MIG) 생성 대상으로 취급된다.

2. 설계 원칙 / Contract Design Principles

2.1 역할 기반 분리

모든 데이터 타입은 다음 중 하나의 역할을 가진다.

역할

설명

Primitive Type

의미적 분류를 위한 기본 타입

Definition

전투 중 불변인 정적 데이터

Runtime State

전투 중 변화하는 내부 상태

Snapshot

외부 관측용 상태 표현

Request / Result

상태 전이를 요청하거나 그 결과를 전달

하나의 타입이 여러 역할을 동시에 가지지 않도록 설계한다.

2.2 단방향 상태 노출

- 
Runtime State → Snapshot 변환은 허용


- 
Snapshot → Runtime State 역변환은 허용하지 않는다


- 
외부 시스템은 Runtime State를 직접 수정하지 않는다


이는 상태 무결성과 테스트 가능성을 우선하기 위한 제약이다.

2.3 변경 감지 기준

- 
필드 추가: 대체로 Non-breaking (Spec 생성 대상)


- 
필드 제거 또는 의미 변경: Breaking (Spec + Migration 생성 대상)


- 
계산 규칙, 해석 방식 변경: Behavior change


3. 기본 타입 / Primitive Types

3.1 SymbolType

- 
Square (□)


- 
Triangle (△)


- 
Circle (○)


의미

- 
전투 판정(상성) 전용 속성 타입


- 
카드의 역할(CardRole)과 독립


- 
심볼 자체에 아키타입이나 역할 의미를 부여하지 않는다


3.2 CardRole

- 
Attack


- 
Defense


- 
Skill


의미

- 
카드의 기능적 분류


- 
심볼과 결합하여 카드의 정체성을 구성하되,
심볼에 공격/방어 의미를 강제하지 않는다


3.3 CombatOutcome

- 
Win


- 
Draw


- 
Lose


의미

- 
심볼 상성 판정 결과


- 
효과 실행 규칙과 적 행동 적용 규칙의 입력값


4. 정적 데이터 / Definitions

4.1 CardDefinition

CardDefinition은 전투 중 불변인 카드 템플릿이다.

권장 필드

- 
Id: string (unique)


- 
Name: string


- 
Symbol: SymbolType


- 
Role: CardRole


- 
Cost: int


- 
BaseValue: int


- 
WinBonusValue: int


- 
BaseEffects: List<EffectRef>


- 
WinEffects: List<EffectRef>


- 
Description: string (optional)


불변 조건

- 
Id는 전역 유일해야 한다


- 
카드의 동작은 EffectRef 조합으로만 정의한다
(카드별 하드코딩 로직 금지)


4.2 EffectRef

EffectRef는 CardDefinition 내에서 실행할 효과를 참조한다.

필드

- 
EffectId: string


- 
ValueOverride: int? (optional)


- 
Formula: string? (optional, 확장용)


의미

- 
EffectId는 EffectRegistry를 통해 실행 함수로 해석된다


- 
ValueOverride가 존재하면 해당 값이 ctx.Value로 사용된다


- 
Formula 도입은 Behavior change로 취급하며 MIG 문서로 관리한다


4.3 EnemyDefinition (선택)

Enemy 정의를 정적 데이터로 분리할 수 있다.
프로토타입 단계에서는 EnemyState에 포함시킬 수 있다.

5. 런타임 데이터 / Runtime State

5.1 CardInstance

전투 중 덱/손패/버림 더미에 존재하는 카드의 실체다.

필드

- 
InstanceId: string (unique per run)


- 
Definition: CardDefinition (reference)


- 
Level: int (optional)


- 
UpgradeCount: int (optional)


불변 조건

- 
InstanceId는 한 전투 세션 내에서 유일


- 
동일 Definition을 참조하는 Instance가 여러 개 존재할 수 있다


5.2 PlayerState

필드

- 
MaxHp: int


- 
Hp: int


- 
Energy: int


- 
Deck: List<CardInstance>


- 
Hand: List<CardInstance>


- 
Discard: List<CardInstance>


의미

- 
Deck / Hand / Discard는 카드 순환을 표현


- 
카드 이동 규칙은 Engine 책임


권장 불변 조건

- 
0 ≤ Hp ≤ MaxHp


- 
Energy ≥ 0


- 
Deck / Hand / Discard 간 InstanceId 중복 없음


5.3 EnemyCard

적 덱을 구성하는 최소 단위.

필드 (최소)

- 
Id: string (optional)


- 
Symbol: SymbolType


- 
Power: int? (optional)


5.4 EnemyState

필드

- 
Id: string


- 
Name: string


- 
MaxHp: int


- 
Hp: int


- 
AttackValue: int


- 
Deck: List<EnemyCard>


- 
Discard: List<EnemyCard> (optional)


- 
LastUsedSymbol: SymbolType? (optional)


의미

- 
Deck은 심볼 선택의 근거이며 사용 시 소모된다


- 
리필 정책은 별도 Feature Spec 범위에서 정의된다


권장 불변 조건

- 
0 ≤ Hp ≤ MaxHp


- 
AttackValue ≥ 0


5.5 Status Containers (확장)

상태이상(Bleed, Vulnerable 등)은 확장 기능이다.

- 
Dictionary<string, int>


- 
또는 별도 StatusState 모델


정형화 시 Migration 필수.

6. API 계약 / Request & Result

6.1 BattleInitData

필드

- 
PlayerState: PlayerState


- 
EnemyState: EnemyState


- 
Seed: int? (optional)


규칙

- 
Initialize는 Host가 구성한 초기 상태를 그대로 수용


- 
초기 덱/손패 구성 정책은 Host 책임


6.2 ResolveRequest

필드

- 
CardInstanceId: string


- 
SlotIndex: int


규칙

- 
CardInstanceId는 반드시 Hand에 존재해야 한다


- 
SlotIndex는 엔진에서 의미를 강제하지 않는다
(로그 / 분석 / UI 보조 정보)


6.3 ResolveResult

권장 필드

- 
Snapshot: BattleSnapshot


- 
Outcome: CombatOutcome


- 
PlayerSymbol: SymbolType


- 
EnemySymbol: SymbolType


- 
DamageDealtToEnemy: int


- 
DamageDealtToPlayer: int


- 
EventTags: List<string>


의미

- 
Snapshot은 ResolveCard 이후의 최신 상태


- 
Damage 값은 결과 요약용 best-effort 정보


- 
EventTags는 UI/연출/분기 힌트


EventTags 예시

- 
MissingEffect:{effectId}


- 
EnergyInsufficient


- 
EnemyActionNegated


- 
CardResolved


- 
TurnEnded


EventTags 의미 변경은 Behavior change로 취급한다.

6.4 NextTurnResult

필드

- 
Snapshot: BattleSnapshot


- 
EventTags: List<string>


규칙

- 
NextTurn은 턴 증가 및 턴 초기화를 수행


- 
세부 정책은 Host/게임 모드에서 확장 가능


6.5 BattleSnapshot

필드

- 
Player: PlayerState (copy)


- 
Enemy: EnemyState (copy)


- 
TurnNumber: int


- 
IsBattleEnded: bool


- 
IsPlayerDead: bool


- 
IsEnemyDead: bool


규칙

- 
읽기 전용 관측 뷰로 취급


- 
내부 상태와 참조를 공유하지 않는다
(shallow copy 또는 immutable 전략 선택)


7. 오류 처리 계약 / Error Handling

상황

처리

Hand에 없는 CardInstanceId

InvalidOperationException

Energy 부족

상태 변화 없음 + EventTags에 EnergyInsufficient

effectId 누락

해당 효과 스킵 + EventTags에 MissingEffect:{effectId}

8. 기술적 제약 / Technical Constraints

본 Data Contracts는 다음 제약을 전제로 설계된다.

- 
Target Framework: .NET Standard 2.1


- 
UnityEngine 참조 금지


- 
Reflection / Dynamic 최소화


- 
테스트 가능성 우선


이에 따라 모든 계약 타입은:

- 
명시적 타입 정의를 가진다


- 
런타임 동적 해석에 의존하지 않는다


- 
테스트 코드에서 직접 생성·비교 가능해야 한다


9. 요약

Data Contracts는 RSP_Core의 공통 언어 사전이다.
여기서 정의된 의미가 바뀌는 순간,
그 변화는 코드 이전에 Spec과 Migration으로 먼저 드러나야 한다.

본 문서는 그 판단을 가능하게 하는 **기준선(baseline)**으로 기능한다.
