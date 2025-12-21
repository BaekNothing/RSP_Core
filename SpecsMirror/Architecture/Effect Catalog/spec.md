# Effect Catalog

Source: https://baeknothing.atlassian.net/wiki/spaces/RSPCore/pages/86704261/Effect+Catalog

1. Purpose

본 문서는 **RSP_Core 및 Host에서 사용 가능한 effectId의 목록과 의미 계약(semantic contract)**을 정의한다.

Effect Catalog는 다음을 목적으로 한다.

- 
effectId의 존재와 의미를 단일 기준으로 고정


- 
Effect System Architecture에서 정의된 실행 규칙을 구체적인 효과 단위로 확정


- 
Spec / Migration 자동 생성을 위한 변경 감지 기준 제공


effectId의 의미 변경, 적용 범위 변경, 값 해석 변경은
모두 Behavior change로 취급되며 Migration(MIG) 문서 생성 대상이다.

2. Conventions

2.1 effectId Naming Rules

- 
소문자 스네이크 케이스 사용


- 
동사 + 목적어 형태 권장


예:

- 
attack_damage


- 
defense_percent


- 
status_bleed


2.2 Value Semantics

- 
EffectRef.ValueOverride가 존재할 경우 해당 값을 사용한다


- 
없을 경우 CardDefinition.BaseValue를 사용한다


- 
Value의 단위/해석은 각 effectId의 계약에 의해 정의된다


Value 해석 방식 변경은 Behavior change로 취급된다.

2.3 Outcome Applicability 표기 규칙

Outcome 적용 범위는 다음 약어로 표기한다.

- 
W: Win


- 
D: Draw


- 
L: Lose


예:

- 
W,D → Win / Draw에서 실행


- 
W → Win에서만 실행


Outcome 적용 범위 변경은 Behavior change다.

2.4 Tagging 규칙

Effect 실행 중 오류 발생 시:

- 
EffectError:{effectId}:{message}
→ EventTags에 추가


등록되지 않은 effectId 사용 시:

- 
EffectNotFound:{effectId}
→ EventTags에 추가 후 해당 Effect는 스킵


침묵 실패(silent failure)는 허용되지 않는다.

3. Built-in Effects

아래 effectId들은 RSP_Core에 기본 제공되는 Built-in Effects다.
이들은 Reference Implementation 성격을 가지며,
의미 계약은 본 문서를 기준으로 고정된다.

effectId

설명

Value 의미

Outcome 적용

비고

attack_damage

적 HP에 피해를 준다

피해량

W,D

기본 공격 피해

attack_bonus_damage

적 HP에 추가 피해를 준다

피해량

W

attack_damage와 동일 의미, 보너스 전용

defense_percent

플레이어 방어율(%)을 증가시킨다

증가할 방어율(%)

W,D

턴 내 누적, 최대 100% 캡

skill_draw

플레이어 덱에서 카드를 드로우한다

드로우 수

W,D

덱 고갈 시 discard 재섞기

status_bleed

적에게 bleed 스택을 부여한다

스택 수

W,D

엔진은 스택만 기록, 처리는 Host 책임

공통 불변 조건

- 
Effect 실행 순서는 CardDefinition에 정의된 EffectRef 순서를 따른다


- 
Built-in Effect의 동작 의미 변경은 Migration 필수


4. Host-defined Effects (Optional)

Host 게임 규칙에 따라 추가되는 effectId는
다음 계약을 반드시 만족해야 한다.

필수 규칙

- 
고유한 식별자 사용 (Built-in / 타 Host 효과와 충돌 금지)


- 
Value의 의미를 명확히 정의


- 
Outcome 적용 범위를 명시


- 
EffectContext의 의미를 변경하지 않는다


Host-defined Effect는 **확장(Extension)**으로 취급되며,
의미 변경 시 Migration 대상이 된다.

예시

effectId

설명

Value 의미

Outcome 적용

비고

lifesteal

적에게 피해를 주고 일부를 체력으로 회복

피해량

W,D

회복 비율은 Host 정의

5. Deprecations & Aliases

EffectId의 이름 변경 또는 폐기는
점진적 전환을 전제로 관리한다.

old_id

new_id

상태

비고

없음

-

-

현재 폐기/별칭 없음

정책

- 
Deprecated effectId는 즉시 제거하지 않는다


- 
Alias 또는 Migration 가이드를 제공한다


- 
완전 제거 시 Migration 필수


6. 변경에 대한 판단 기준 요약

변경 유형

판단

신규 effectId 추가

Extension

effectId 의미 변경

Behavior change + MIG

Outcome 적용 범위 변경

Behavior change + MIG

Value 해석 변경

Behavior change + MIG

문서 설명 보강

Clarification

7. 요약

Effect Catalog는 RSP_Core 전투 규칙의 어휘 사전이다.
여기서 정의된 effectId와 의미는 코드보다 먼저 고정되어야 하며,
변경은 반드시 Spec과 Migration으로 드러나야 한다.

본 문서는 Effect System Architecture와 함께
RSP_Core의 확장 가능성과 안정성을 동시에 보장하는 기준선으로 기능한다.
