# Projekto „Prieigos taškai“ Aprašymas

## 1. Sprendžiamo Uždavinio Aprašymas

### 1.1. Sistemos Paskirtis

Projekto tikslas – palengvinti informacijos rinkimą apie prieigos taškus, kurie bus panaudoti vietinėje navigacijos sistemoje.

**Veikimo principas:**  
Sistemą sudaro dvi dalys:

- Internetinė aplikacija (naudojama neregistruotų vartotojų, registruotų vartotojų ir administratorių)
- Aplikacijų programavimo sąsaja (API)

**Vartotojų galimybės:**

- **Neregistruoti vartotojai** – gali tik peržiūrėti informaciją.
- **Registruoti vartotojai** – gali pridėti ir redaguoti informaciją apie jiems priskirtus skanavimo taškus.
- **Administratoriai** – tvirtina registracijas, valdo pastatus, jų planus, skanavimo taškus ir priskiria juos vartotojams.

---

### 1.2. Funkciniai Reikalavimai

#### Neregistruotas naudotojas gali:

1. Peržiūrėti informaciją apie prieigos taškus  
2. Prisijungti prie sistemos  

#### Registruotas naudotojas gali:

1. Atsijungti nuo sistemos  
2. Užsiregistruoti sistemoje  
3. Redaguoti naudotojui priskirtų skanavimo taškų informaciją  
4. Pridėti informaciją apie priskirtus skanavimo taškus  

#### Administratorius gali:

1. Patvirtinti vartotojo registraciją  
2. Atmesti vartotojo registraciją  
3. Pridėti pastatą  
4. Ištrinti pastatą  
5. Pridėti pastato planus  
6. Ištrinti pastato planus  
7. Pridėti skanavimo taškus  
8. Ištrinti skanavimo taškus  
9. Ištrinti konkretaus skanavimo taško prieigos taškų informaciją  
10. Priskirti naudotojui skanavimo taškus


## 2. Paleidimo instrukcijos
Programos reikalingos paleisti projektą:
- Git
- Docker
Komandos reikalingos paleisti projektą:
```
git clone https://github.com/SmikisBilda/Saitynai/
cd Saitynai
docker-compose build
docker-compose up -d
```
Programa turėtu būti pasiekiama per:
```
http://localhost:8080/swagger
```


