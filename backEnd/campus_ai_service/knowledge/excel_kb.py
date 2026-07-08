import pandas as pd
import os
import torch
import re
import numpy as np
import torch.nn.functional as F
from rank_bm25 import BM25Okapi
from transformers import AutoModel, AutoTokenizer, AutoModelForSequenceClassification
from langchain_community.vectorstores import FAISS
from langchain_huggingface import HuggingFaceEmbeddings

# ==================== 固定配置 ====================
KB_PATH = os.path.join(os.path.dirname(__file__), "../data/knowledge/campus_knowledge.xlsx")
#VECTOR_MODEL_PATH = "./models/bge-small-zh-v1.5"
#VECTOR_MODEL_PATH = "./models/bge-reranker-base"
VECTOR_MODEL_PATH = "D:/PocketCampus/campus_ai_service/models/bge-small-zh-v1.5"
RERANK_MODEL_PATH = "D:/PocketCampus/campus_ai_service/models/bge-reranker-base"

MYSQL_TEMPLATE = {
    "host": "localhost",
    "port": 3306,
    "user": "root",
    "password": "your_password",
    "database": "campus_ai",
    "table": "knowledge_base"
}

class CampusRAGKB:
    def __init__(self):
        self.df = None
        self.all_data = []
        self.category_map = {}

        self.tokenizer = None
        self.vector_model = None
        self.rerank_tokenizer = None
        self.rerank_model = None

        self.bm25 = None
        self.vector_store = None
        
        # 完全对齐你Excel的分类
        self.npc_categories = {
            "volunteer": "迎新志愿者，负责新生报到、流程指引、路线咨询",
            "dorm_manager": "楼管阿姨，负责宿舍管理、门禁钥匙、报修、晚归、卫生",
            "nurse": "校医院护士，负责看病、开药、发烧受伤处理",
            "gardener": "园艺工人，负责校园绿化、花草修剪、环境养护",
            "librarian": "图书管理员，负责图书馆借书、还书、预约、座位",
            "security": "校园保安，负责门禁、安全、巡逻、失物招领",
            "grass_cow": "草地牛，搞笑互动",
            "echo": "账户客服小精灵，负责账号服务、安全管理、角色管理、游戏设置、客服支持、开发工具、校园导览、智能应答",  
        }
        self.npc_embeddings = {}

        self.init_all()

    def init_all(self):
        self.load_knowledge()
        self.load_vector_model()
        self.load_rerank_model()
        self.init_npc_embeddings()
        self.init_multiple_retrieval()
        print("✅ 多路召回+重排序+NPC路由 初始化完成！")

    def load_knowledge(self):
        self.df = pd.read_excel(KB_PATH)
        self.all_data = []
        for _, row in self.df.iterrows():
            self.all_data.append({
                "category": str(row.get("category", "default")).strip(),
                "question": str(row.get("question", "")).strip(),
                "answer": str(row.get("answer", "")).strip()
            })

        categories = self.df["category"].unique()
        print(f"✅ 知识库加载：{len(self.all_data)} 条，分类：{list(categories)}")

    def load_vector_model(self):
        self.tokenizer = AutoTokenizer.from_pretrained(VECTOR_MODEL_PATH, local_files_only=True)
        self.vector_model = AutoModel.from_pretrained(VECTOR_MODEL_PATH, local_files_only=True)
        print("✅ 向量模型加载完成")

    def load_rerank_model(self):
        self.rerank_tokenizer = AutoTokenizer.from_pretrained(RERANK_MODEL_PATH, local_files_only=True)
        self.rerank_model = AutoModelForSequenceClassification.from_pretrained(
            RERANK_MODEL_PATH, local_files_only=True
        )
        self.rerank_model.eval()
        print("✅ 重排序模型加载完成")

    def init_npc_embeddings(self):
        for cat, desc in self.npc_categories.items():
            self.npc_embeddings[cat] = self.encode_single(desc)

    def encode_single(self, text):
        inputs = self.tokenizer(text, padding=True, truncation=True, return_tensors="pt", max_length=512)
        with torch.no_grad():
            emb = self.vector_model(**inputs)[0][:, 0]
        return F.normalize(emb, p=2, dim=1)

    def init_multiple_retrieval(self):
        corpus = [item["question"] for item in self.all_data]
        tokenized_corpus = [self.clean_question(q).split() for q in corpus]
        self.bm25 = BM25Okapi(tokenized_corpus)

        embeddings = HuggingFaceEmbeddings(model_name=VECTOR_MODEL_PATH)
        texts = [item["question"] for item in self.all_data]
        self.vector_store = FAISS.from_texts(texts, embeddings)

    def clean_question(self, q):
        stop_words = ["学校", "请问", "有没有", "怎么", "如何", "哪里", "的", "吗", "呢", "了","你知不知道","能不能告诉我","能否告诉我","能不能说说","能否说说","能不能讲讲","能否讲讲","我想知道","我想请问","我想问一下","请问一下","麻烦告诉我","麻烦说说","麻烦讲讲"]
        q = q.lower()
        for w in stop_words:
            q = q.replace(w, "")
        q = re.sub(r'[^\w\s]', '', q).strip()
        return q

    def rerank(self, query, candidates, top_n=3):
        pairs = [[query, cand] for cand in candidates]
        inputs = self.rerank_tokenizer(
            pairs, padding=True, truncation=True, max_length=512, return_tensors="pt"
        )
        with torch.no_grad():
            scores = self.rerank_model(**inputs).logits.view(-1)
        ranked_indices = np.argsort(scores.numpy())[::-1][:top_n]
        return [candidates[i] for i in ranked_indices], [scores[i].item() for i in ranked_indices]

    # ====================== 【修复后：严格分类隔离】 ======================
    def search(self, question: str, category: str = "default", threshold: float = 0.3):
        if not self.all_data:
            return ""

        try:
            clean_q = self.clean_question(question)
            tokenized_q = clean_q.split()

            # 1. 全量召回
            bm25_top10 = self.bm25.get_top_n(tokenized_q, [d["question"] for d in self.all_data], n=10)
            vector_top10 = self.vector_store.similarity_search(question, k=10)
            vector_top10 = [doc.page_content for doc in vector_top10]

            # 2. 合并去重
            candidates = list(set(bm25_top10 + vector_top10))
            if not candidates:
                return ""

            # 3. 重排序
            ranked_cands, scores = self.rerank(question, candidates, top_n=5)

            # 4. 按分类过滤（严格隔离）
            valid_answers = []
            for idx, score in enumerate(scores):
                if score < threshold:
                    continue
                q_text = ranked_cands[idx]
                match_item = next((d for d in self.all_data if d["question"] == q_text), None)
                if not match_item:
                    continue

                # ====================== 核心修复 ======================
                # default 只能看 default
                if category == "default":
                    if match_item["category"] != "default":
                        continue
                # 其他NPC只能看自己
                else:
                    if match_item["category"] != category:
                        continue
                # ======================================================

                valid_answers.append(match_item["answer"])

            return "\n".join(valid_answers) if valid_answers else ""

        except Exception as e:
            print(f"检索异常：{e}")
            return ""

    # 向量相似度路由（非关键词）
    def route_to_best_npc(self, question):
        q_emb = self.encode_single(question)
        best_cat = None
        best_score = 0.45

        for cat, emb in self.npc_embeddings.items():
            score = F.cosine_similarity(q_emb, emb).item()
            if score > best_score:
                best_score = score
                best_cat = cat
        return best_cat

campus_kb = CampusRAGKB()