# Trigger 상호작용 설정

## 역할

| 컴포넌트 | 부착 위치 | 역할 |
|---|---|---|
| `InteractionObject` | 감지 영역 | Trigger 진입/이탈 등록, 안내 문구, 실행 이벤트 |
| `MiningInteractionObject` | 광석의 감지 영역 | 홀드 완료 시 `MiningTarget`의 보상 실행 |
| `ShopInteractionObject` | 상점의 감지 영역 | E 키를 누르면 연결된 `ShopManager` 열기 |
| `InteractionManager` | 별도 Manager 오브젝트 | 연결된 플레이어 기준 최근접 대상 선택, 입력·홀드 시간 관리 |
| `InteractionUI` | 안내 UI | 대상 문구, 키 힌트, 진행도 표시 |
| `PlayerMiningInteraction` | 플레이어 | 곡괭이 애니메이션 실행 |

`MiningTarget`에는 기존 보상 이벤트와 채굴 시간이, `ShopManager`에는 상점 UI와 거래 로직이 남아 있습니다.

## 기존 방식과의 차이

- 이전: 씬 이름에 따라 입력을 분기하고, 매 프레임 광석 목록과 거리를 비교했습니다.
- 현재: 각 감지 영역의 `OnTriggerEnter2D` / `OnTriggerExit2D`가 후보를 등록·해제합니다. 후보끼리의 거리 비교는 대상 선택에만 사용합니다.
- Trigger는 감지만 합니다. 상점은 한 번 누르기, 채굴은 홀드로 실행합니다.
- 같은 키를 누른 채 범위에 진입하거나 다른 대상에 접근해도 자동 실행하지 않습니다. 키를 뗐다가 다시 눌러야 합니다.
- 홀드 중에는 선택한 대상을 유지합니다. 범위를 벗어나거나 대상이 비활성화되면 진행과 애니메이션을 중단합니다.
- 상점이나 인벤토리가 열린 동안에는 다른 상호작용과 공격 입력을 막습니다.

## 범위를 바꾸는 방법

씬 계층에서 대상 아래의 **InteractionZone**을 선택하고 `BoxCollider2D`의 **Edit Collider**로 모양과 크기를 조절합니다. **Is Trigger**는 켜져 있어야 합니다.

```text
Player
├─ Rigidbody2D + Collider2D
└─ PlayerInputHandler

GameManager (또는 별도 Manager 오브젝트)
└─ InteractionManager → Player에 실제 플레이어 연결

광석 오브젝트 (MiningTarget)
└─ InteractionZone (BoxCollider2D + MiningInteractionObject)

상점 오브젝트 (BoxCollider2D: Is Trigger 끔, 통과 방지)
└─ InteractionZone (BoxCollider2D + ShopInteractionObject)

InteractionCanvas
└─ InteractionPrompt (InteractionUI)
```

물리 충돌용 콜라이더와 상호작용 영역은 별개입니다. 감지 영역을 넓혀도 벽이나 플레이어의 물리 충돌 모양은 바뀌지 않습니다.

상점 본체의 `BoxCollider2D`는 **Is Trigger를 끄고**, 자식 `InteractionZone`의 콜라이더는 **Is Trigger를 켭니다**. 감지 영역은 충돌 영역 바깥까지 넓혀야 상점 앞에 멈춰 선 상태에서도 E 키를 사용할 수 있습니다. `ShopInteractionObject`는 자식 감지 영역에만 둡니다. 플레이어는 `FixedUpdate`에서 `Rigidbody2D.MovePosition`으로 이동하므로 본체 콜라이더에 막힙니다.

`PlayerInputHandler`의 매니저 필드에는 별도 오브젝트의 `InteractionManager`를 연결합니다. Trigger는 플레이어의 입력 핸들러를 통해 해당 매니저에 등록됩니다. 거리 계산과 채굴 애니메이션도 매니저의 위치가 아닌 연결된 플레이어를 사용합니다.

매니저는 씬 이동 시 유지됩니다. 각 씬의 Manager에는 해당 씬의 Player, Interaction UI, World Camera, Inventory를 연결해 둡니다. 새 씬의 매니저가 이 참조를 기존 매니저에 전달하고 자신의 중복 컴포넌트를 제거하므로, 런타임에는 하나만 동작합니다.

Trigger 이벤트에는 양쪽의 Collider2D와 적어도 한쪽의 Rigidbody2D가 필요합니다. 플레이어의 기존 Rigidbody2D/콜라이더를 유지했습니다. [Unity 공식 문서](https://docs.unity.com/en-us/engine/6000.0/script-reference/unityengine/monobehaviour/ontriggerenter2d)

## 새 오브젝트 추가 예시

### 상점

1. 월드 상점 아래에 `InteractionZone` 자식을 만듭니다.
2. `ShopInteractionObject`를 추가합니다. 기본 BoxCollider2D가 함께 추가됩니다.
3. `Shop Manager` 필드에 실제 상점 매니저를 연결합니다.
4. 안내 문구와 Trigger 크기를 조절합니다. 플레이어 입력 코드는 수정하지 않습니다.

### 채굴

1. 광석 아래의 `InteractionZone`에 `MiningInteractionObject`를 추가합니다.
2. `Mining Target`에 해당 광석의 `MiningTarget`을 연결합니다.
3. 채굴 시간과 보상은 `MiningTarget`에서, 감지 범위와 안내 문구는 영역에서 조절합니다.

### 문, NPC 등 간단한 기능

`InteractionObject`를 추가하고 **On Interacted** 이벤트에 실행할 함수를 연결합니다. `Hold Duration`이 0이면 한 번 누르기, 양수이면 지정 시간 홀드입니다. 채굴처럼 별도 동작이 필요하면 `InteractionObject`를 상속하여 `Interact()`와 `SetHolding()`을 구현합니다.

## 안내 UI 변경

`InteractionPrompt`의 `InteractionUI`에서 라벨·진행 막대·키 배경을 연결합니다. 키 표시를 바꾸려면 `Interaction Key`를 수정하고 Input System 바인딩도 함께 바꿉니다. 행동 문구는 각 `InteractionObject`에서 변경합니다.
