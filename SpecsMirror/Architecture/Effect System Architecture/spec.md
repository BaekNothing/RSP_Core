# Effect System Architecture

Source: https://baeknothing.atlassian.net/wiki/spaces/RSPCore/pages/86704198/Effect+System+Architecture

1. 목적과 지위

본 문서는 RSP_Core의 Effect System 구조와 의미적 계약을 정의한다.
Effect System은 RSP_Core에서 **의도적으로 허용된 유일한 확장 지점(extension point)**이다.

Effect System은 다음을 목적으로 설계된다.

- 
카드 및 전투 규칙의 행동 로직을 데이터 기반으로 분리


- 
Core 전투 흐름을 변경하지 않고 게임별 규칙 확장 허용


- 
Spec / Migration 자동 생성을 위한 명확한 변경 기준 제공


Effect System은 “편의 기능”이 아니라,
RSP_Core Architecture의 핵심 구성 요소로 취급된다.

2. Effect System의 역할 범위

Effect System은 다음 책임을 가진다.

- 
effectId 문자열과 실행 로직 간의 매핑


- 
효과 실행 순서의 일관성 유지


- 
효과 실행 중 발생한 오류의 안전한 보고(EventTags)


Effect System은 다음을 의도적으로 책임지지 않는다.

- 
효과의 시각적 표현


- 
상태이상 틱 처리


- 
밸런싱 판단


- 
효과 간 우선순위 정책의 동적 변경


이 경계를 넘는 요구사항은 Architecture 변경으로 간주된다.

3. 구성 요소 개요

Effect System은 개념적으로 다음 요소들로 구성된다.

3.1 EffectRegistry

EffectRegistry는 다음 계약을 따른다.

- 
effectId (string) → EffectHandler (delegate)


- 
effectId는 의미적 식별자이며, 실행 로직과 분리된다


- 
동일 effectId 재등록 시 마지막 등록이 유효


EffectRegistry는 확장 지점이며,
Host 또는 테스트 코드에서 효과를 주입할 수 있다.

3.2 EffectHandler (CardEffectDelegate)

EffectHandler는 다음 특성을 가진다.

- 
EffectContext를 입력으로 받는다


- 
반환값을 가지지 않는다


- 
Snapshot을 직접 변경(mutate)한다


- 
예외를 던질 수 있다


EffectHandler의 구현은 Engine 외부 책임이며,
Engine은 실행 순서와 오류 처리만을 보장한다.

3.3 EffectContext

EffectContext는 효과 실행 시 제공되는 유일한 실행 맥락이다.

포함 정보의 의미는 다음과 같다.

- 
Snapshot: 현재 전투 상태 (mutable)


- 
SourceCard: 효과를 발생시킨 카드


- 
Value: 계산된 효과 값


- 
Formula: 예약된 확장 필드


- 
Outcome: 해당 카드의 전투 판정 결과


EffectContext의 필드 의미 변경은
고위험 Architecture 변경으로 취급된다.

4. Effect 실행 규칙

4.1 실행 시점

Effect는 카드 해결 과정 중 다음 규칙에 따라 실행된다.

- 
Win:

- 
BaseEffects 실행


- 
WinEffects 실행



- 
Draw:

- 
BaseEffects 실행



- 
Lose:

- 
어떠한 Effect도 실행되지 않음



이 실행 규칙은 불변식이다.

4.2 실행 순서

- 
EffectRef 목록에 정의된 순서를 그대로 따른다


- 
Engine은 순서를 변경하거나 병렬 실행하지 않는다


Effect 실행 순서 변경은
Behavior change + Migration 대상이다.

4.3 값 결정 규칙

- 
EffectRef에 ValueOverride가 존재하면 해당 값을 사용


- 
없으면 CardDefinition의 BaseValue를 사용


Value 계산 규칙 변경은
Spec + Migration 생성 대상으로 취급된다.

5. 오류 처리 정책

Effect 실행 중 오류는 다음 원칙을 따른다.

5.1 Effect 누락

- 
effectId가 Registry에 존재하지 않을 경우


- 
해당 Effect는 스킵


- 
EventTag: MissingEffect:{effectId} 추가


- 
전투 흐름은 중단되지 않음


5.2 Effect 실행 오류

- 
EffectHandler 내부 예외 발생 시


- 
해당 Effect는 실패 처리


- 
EventTag: EffectError:{effectId}:{message} 추가


- 
나머지 Effect 실행은 계속 진행


침묵 실패(silent failure)는 허용되지 않는다.

6. Status와 Effect의 관계

Effect System은 상태이상(Status)을 다음과 같이 취급한다.

- 
Status는 추적만 수행


- 
상태이상의 실제 효과 해석은 Host 책임


- 
Effect는 Status 스택을 추가·감소시킬 수 있음


Status 처리 로직을 Effect System에 포함시키는 변경은
Architecture 변경으로 간주된다.

7. Built-in Effects의 지위

RSP_Core에 포함된 기본 Effect들은 다음 성격을 가진다.

- 
Reference implementation


- 
제거 또는 교체 가능


- 
Effect 실행 규칙 자체에는 영향 없음


Built-in Effect의 동작 의미 변경은
Behavior change로 취급된다.

8. 확장과 변경에 대한 판단 기준

자동화 에이전트는 Effect System 변경을 다음과 같이 해석한다.

Extension (Migration 불필요)

- 
신규 effectId 추가


- 
신규 EffectHandler 등록


- 
기존 EffectContext를 그대로 사용하는 확장


Behavior Change (Migration 필수)

- 
기존 effectId의 의미 변경


- 
EffectContext 필드 의미 변경


- 
Effect 실행 순서 변경


- 
Value 계산 규칙 변경


Clarification

- 
문서 보강


- 
주석/설명 개선


9. 기술적 제약

Effect System은 다음 제약을 따른다.

- 
.NET Standard 2.1


- 
UnityEngine 의존 없음


- 
Reflection / Dynamic 사용 최소화


- 
테스트 코드에서 직접 호출 가능해야 함


이 제약은 자동화 테스트와 결정적 실행을 전제로 한다.

10. 요약

Effect System은 RSP_Core에서 가장 강력하면서도 위험한 확장 지점이다.
따라서 그 구조와 의미는 코드보다 먼저 Architecture 문서로 고정되어야 한다.

본 문서는 Effect 관련 변경이
Spec, Migration, 코드 변경으로 어떻게 파급되는지를 판단하기 위한
**기준선(baseline)**으로 기능한다.
