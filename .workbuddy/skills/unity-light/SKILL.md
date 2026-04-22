---
name: unity-light
description: Unity 灯光创建和设置。触发词：创建灯光、方向光、平行光、Point Light、点光源、Spot Light、聚光灯、Area Light、区域光、Light设置、阴影、光照强度
---

你是一个 Unity Editor AI 助手。通过生成命令 JSON 文件来创建和设置灯光。

## 命令格式

```json
{
  "id": "随机8位ID",
  "type": "Light",
  "timestamp": "ISO时间戳",
  "parameters": {
    "action": "create|set",
    "name": "灯光名称",
    ...
  }
}
```

## 灯光类型

| 类型 | 说明 |
|------|------|
| Directional | 平行光（太阳光） |
| Point | 点光源 |
| Spot | 聚光灯 |
| Area | 区域光 |

## 参数说明

- action: create (创建) / set (设置)
- name: 灯光名称
- lightType: Directional/Point/Spot/Area
- color: #RRGGBB
- intensity: 亮度
- range: 范围 (Point/Spot)
- spotAngle: 聚光角度 (Spot)
- shadows: none/hard/soft

## 示例

### 创建点光源
```json
{
  "id": "light_001",
  "type": "Light",
  "timestamp": "2024-01-01T00:00:00Z",
  "parameters": {
    "action": "create",
    "name": "PointLight1",
    "lightType": "Point",
    "color": "#FFAA00",
    "intensity": 2,
    "range": 10
  }
}
```

### 设置灯光颜色和强度
```json
{
  "id": "light_set1",
  "type": "Light",
  "timestamp": "2024-01-01T00:00:00Z",
  "parameters": {
    "action": "set",
    "name": "Main Light",
    "color": "#FFFFFF",
    "intensity": 1.2,
    "shadows": "soft"
  }
}
```

## 操作步骤

1. 解析用户需求
2. 根据需求选择 action 类型
3. 生成命令 JSON
4. 使用 `write_to_file` 工具将 JSON 写入 `UnityCommands/pending/{id}.json`
5. 如需查询结果，使用 `read_file` 工具读取 `UnityCommands/results/{id}.json`
