# 全局内存：key = 用户ID + 分类 → 彻底隔离
USER_MEMORY = {}

class LocalWindowMemory:
    def __init__(self, k=5):
        self.k = k
        self.history = []

    def save_context(self, inputs, outputs):
        user_input = inputs.get("input", "").strip()
        ai_output = outputs.get("output", "").strip()
        if user_input and ai_output:
            self.history.append({"input": user_input, "output": ai_output})
            while len(self.history) > self.k:
                self.history.pop(0)

    def load_memory_variables(self, inputs):
        if not self.history:
            return "无历史对话"
        history_text = ""
        for i, chat in enumerate(self.history[-5:], 1):
            history_text += f"你：{chat['input']}\n我：{chat['output']}\n"
        return history_text.strip()

# 【关键修复】按 用户+NPC 独立存储记忆
def get_user_memory(player_id: str, category: str):
    if not player_id or not category:
        return None
    key = f"{player_id}_{category}"
    if key not in USER_MEMORY:
        USER_MEMORY[key] = LocalWindowMemory(k=5)
    return USER_MEMORY[key]