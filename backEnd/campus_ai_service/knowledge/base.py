from abc import ABC, abstractmethod

class BaseKnowledge(ABC):
    @abstractmethod
    def load(self):
        pass

    @abstractmethod
    def search(self, question: str):
        pass