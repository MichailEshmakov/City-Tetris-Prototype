# Процесс разработки
Процесс основан на принципе [gitHub flow](https://guides.github.com/introduction/flow/) и устроен следующим образом:
 - В мастеровой ветке всегда должен быть работающий код

 **Руководитель:**
 
 - Определяет, что должно быть сделано на данном этапе. Создает майлстоун с кратким описанием необходимой работы и привязывет к нему дату
 - На каждый требующий исправления баг или требующую добавления фичу, которую можно реализовать независимо от другой на данном этапе разработки, руководитель создает ишью, прикрепляет к майлстоуну
 - Каждое ишью крепится к определенному разработчику, который ее выполняет
 - После выполнения разработчиком задачи собирает проект из ветки develop и проверяет выполнение задачи по ее описанию
 - В случае некорректного выполнения задачи, задача возвращается разработчику на доработку
 - Если в ходе проверки обнауживаются сторонние ошибки, создаются соответсвующие ишью
 - Если задачи в майлстоуне кончились, проверяет соответствие выполненной работы техническому заданию
 - Если обнаруживаются ошибки, создаются соответсвующие ишью
 - Если ошибок не обнаруживается, ветка develop сливается с мастеровой. Создается отметка конце мастеровой ветки

 **Разработчик:**

 - Разработчик, взявший на выполнение задачу совершает вытягивание к себе ветку develop
 - На последнем коммите ветки develop разработчик создает ветку с названием похожим на название задачи и с идентификатором задачи
 - Далее разработчик ведет работу над своей задачей в этой ветке, делает коммиты, название которых на русском языке описывает проделанную в коммитах работу и заканчивается идентификатором задачи, например "добавлено движение камеры #4"
 - Когда разработчик решает, что выполнил задачу, он создает запрос на вытягивание своей ветки в ветку develop проекта
 - В описании запроса ны вятягивание должен быть issue id. Название запроса должно быть похоже на название задачи
 - Если случаются конфликты, разработчик должен вытянуть ветку develop и смержить ее с веткой своей задачи, устранив конфликты, затем запушить изменения в свою ветку на репозитории.
 - Если не остается конфликтов, остальные разработчики проводят ревью и обсуждают код
 - Замечания, написанные во время ревью, должен резолвить только тот, кто их писал
 - Для завершения ревью и слияния требуется одобрение двух проводивших ревью разработчиков
 - На этапе прототипирования ревью не требуется
 - Если все удовлетворены проделанной работой, ветка сливается с веткой develop и удаляется
 - Если требуется доработка, разработчик продолжает работать в своей ветке, и закончив работу снова делает запрос на вытягивание

# Тестирование
Тесты не пишем. Если мы узнаем, как это делать, закончим этап прототипирования, и захотим, вернемся к этому вопросу.
 
# Стиль написания кода
Конкретных требований к стилю нет, но все же рекомендуется писать читаемый код. Соответсвенно, если нет проблем с прочтением кода, на ревью замечания по стилю делать не нужно.
