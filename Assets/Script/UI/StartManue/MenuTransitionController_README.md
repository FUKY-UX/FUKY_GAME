# 主菜单过渡控制器 (MenuTransitionController)

这个脚本用于处理主菜单场景中，当玩家按下任意键后，各个UI元素的动画过渡效果。

## 功能描述

当玩家按下任意键时：
1. 主菜单标志（Logo）会向上飞出屏幕，然后被禁用
2. "点击任意键开始"文字会立即被禁用
3. 主菜单下划装饰UI会向下飞出屏幕，然后被禁用

## 使用方法

1. 将 `MenuTransitionController.cs` 脚本添加到场景中的 `Begin_Canvas` 对象上
2. 在 Inspector 面板中设置必要的UI引用：
   - **Menu Logo**: 主菜单标志UI（需要是RectTransform类型）
   - **Start Prompt**: "点击任意键开始"文字UI（GameObject类型）
   - **Menu Decoration**: 主菜单下划装饰UI（需要是RectTransform类型）
3. 调整动画参数以满足需求
4. 运行场景，按下任意键触发过渡动画

## 参数说明

### UI引用

- **Menu Logo**: 主菜单标志UI，将向上飞出屏幕
- **Start Prompt**: "点击任意键开始"文字UI，将立即禁用
- **Menu Decoration**: 主菜单下划装饰UI，将向下飞出屏幕

### 动画设置

- **Animation Duration**: 动画持续时间（秒），范围从0.5到3秒
- **Animation Curve**: 动画曲线，控制动画的加速和减速效果
- **Logo Move Distance**: 标志向上移动的距离（相对于屏幕高度的倍数）
- **Decoration Move Distance**: 装饰向下移动的距离（相对于屏幕高度的倍数）

## 公共方法

脚本提供了以下可以在其他脚本中调用的公共方法：

- `StartTransition()`: 手动触发过渡动画（如果不想通过按键触发）
- `ResetMenu()`: 重置所有UI元素到初始状态

## 示例代码

```csharp
// 获取菜单过渡控制器组件
MenuTransitionController menuController = GetComponent<MenuTransitionController>();

// 手动触发过渡动画
menuController.StartTransition();

// 重置菜单到初始状态
menuController.ResetMenu();
```

## 注意事项

- 确保所有引用的UI元素都已正确设置，否则脚本将在控制台输出错误并禁用自身
- 动画使用RectTransform的anchoredPosition属性，确保UI元素使用了适当的锚点设置
- 动画完成后，UI元素会被自动禁用（SetActive(false)）
- 如果需要在动画完成后执行其他操作（如加载新场景），可以修改脚本或使用Unity的事件系统
