# Examples: Data Declaration & Initialization

Source: https://baeknothing.atlassian.net/wiki/spaces/RSPCore/pages/86835295/Examples+Data+Declaration+Initialization

1. Purpose

본 문서는 RSP_Core 초기화(Initialize)를 위해 Host가 준비해야 하는 데이터 선언 방식의 예시를 제공한다.
Player / Enemy / Cards / Effects를 CSV 또는 JSON으로 정의하고, 이를 런타임 DTO(BattleInitData, PlayerState, EnemyState, CardDefinition, CardInstance, EnemyCard)로 변환하는 흐름을 설명한다.

중요: 본 문서는 “저장 포맷”을 강제하지 않는다.
다만, Data Contracts의 의미를 보존하는 최소 스키마를 예시로 제시한다.

2. Initialization Data Model (요약)

Host는 최소한 아래를 준비한다.

- 
CardDefinition[] (정적 카드 템플릿)


- 
PlayerState (CardInstance로 구성된 Deck/Hand/Discard)


- 
EnemyState (EnemyCard로 구성된 Deck/Discard + AttackValue 등)


- 
BattleInitData (PlayerState/EnemyState + MaxSlotsPerTurn/InitialHandSize 등)


3. JSON Example (권장: 단일 파일 또는 여러 파일)

3.1 cards.json (CardDefinition 목록)
wide760
- 
symbol은 SymbolType 값(Square|Triangle|Circle)


- 
role은 CardRole 값(Attack|Defense|Skill)


- 
Effect는 effectId로만 의미가 결정됨(카드별 하드코딩 금지)


3.2 enemy.json (EnemyState + Enemy Deck)
wide760
- 
power가 없거나 null이면, 엔진 정책에 따라 attackValue를 사용할 수 있다(프로토타입 단계).


- 
discard는 선택(없으면 빈 배열로 취급)


3.3 player.json (PlayerState + Deck Instances)
wide760
- 
cardId는 CardDefinition.id를 참조한다.


- 
instanceId는 전투 세션 내 유일해야 한다.


3.4 battle_init.json (BattleInitData)
wide760
- 
seed는 결정적 실행/테스트를 위해 권장(없으면 비결정적)


4. CSV Example (운영/툴링 친화)

CSV는 “정적 정의”와 “런타임 인스턴스”를 분리하는 편이 안전하다.

4.1 cards.csv (CardDefinition)
wide760
4.2 card_effects.csv (EffectRef 목록)
wide760
- 
phase: base 또는 win


- 
order: 실행 순서(정렬 기준)


- 
valueOverride/formula는 선택


4.3 player_deck.csv (CardInstance 선언)
wide760
4.4 enemy_deck.csv (EnemyCard 선언)
wide760
4.5 battle_config.csv (BattleInitData + Player/Enemy 기본 스탯)
wide760
5. Host-side Assembly Rules (중요)

데이터를 DTO로 조립할 때, Host는 아래 규칙을 따른다.

5.1 CardDefinition Registry

- 
Dictionary<string, CardDefinition>를 구성한다.


- 
cardId 참조가 모두 존재하는지 검증한다.


5.2 CardInstance Composition

- 
player_deck.csv/json의 cardId로 CardDefinition을 찾아 CardInstance.Definition에 연결한다.


- 
instanceId 중복은 금지한다.


5.3 Initial Hand Policy

초기 손패 구성은 두 방식 중 하나를 선택한다(프로토타입 단계 권장: A).

- 
A) deck만 채우고 hand는 빈 상태로 두기 → Initialize에서 셔플+드로우로 생성


- 
B) hand를 미리 구성 → initialHandSize와의 충돌 규칙을 명시해야 함(권장하지 않음)


5.4 Enemy Deck Policy

- 
EnemyState.deck가 비어있을 때의 동작(기본 심볼/attackValue 사용 등)은 명시된 엔진 정책을 따른다.


- 
power가 없으면 attackValue를 사용하는 정책을 선택할 수 있다(일관성 필요).


6. Minimal “Hello Battle” Example (논리 흐름)

초기화 시 Host는 다음 순서로 조립한다.

- 
CardDefinition 로드( JSON/CSV → CardDefinition[] )


- 
EffectRef 연결( CSV면 card_effects로 결합 )


- 
Player CardInstance 로드( cardId → CardDefinition 매핑 )


- 
Enemy Deck 로드


- 
BattleInitData 조립


- 
engine.Initialize(initData) 호출


7. What to put under Version Control

권장:

- 
cards.json 또는 cards.csv + card_effects.csv


- 
enemy_*.json 또는 enemy_deck.csv + enemy metadata


- 
player_starter_deck.csv (스타터 덱 등 “콘텐츠”)


비권장:

- 
전투 중 생성되는 런타임 Snapshot 저장(리플레이 목적이 아니면)
