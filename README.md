# 🚀 SepsisNlp - Pipeline Inteligente de Processamento Clínico

O **SepsisNlp** é uma plataforma de alta performance dedicada à ingestão, processamento e análise de grandes volumes de dados clínicos hospitalares. O projeto foca em arquitetura orientada a eventos para garantir robustez e escalabilidade no tratamento de registros assistenciais.

---

## 🏗 Arquitetura do Pipeline

O sistema utiliza mensageria assíncrona para garantir que a ingestão de dados não bloqueie o processamento principal.



---

## 🛠 Tecnologias e Stack

* **Backend:** C#, .NET 8, MediatR, MassTransit.
* **Mensageria:** RabbitMQ (Processamento assíncrono).
* **Banco de Dados:** PostgreSQL (Schemas segregados: `clinical` & `security`).
* **Pipeline de ETL:** Regex, Streaming de arquivos (Large File Upload), Normalização UTF-8.
* **Infraestrutura:** Docker.

---

## 📋 Funcionalidades Principais

* **Ingestão de Carga Massiva:** Endpoint otimizado para processamento de arquivos CSV gigantescos (Streaming via Kestrel).
* **Extração Inteligente:** Parsing de assinaturas médicas e dados clínicos não estruturados através de expressões regulares (Regex).
* **Mensageria:** Integração via MassTransit para desacoplamento e processamento resiliente.
* **Privacidade:** Anonimização de dados sensíveis seguindo boas práticas de segurança clínica.

---

## 🚀 Como Executar

1. **Clone o repositório:**
   ```bash
   git clone [https://github.com/yTznn/SepsisNlp.git](https://github.com/yTznn/SepsisNlp.git)