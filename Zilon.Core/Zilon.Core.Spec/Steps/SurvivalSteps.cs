﻿using FluentAssertions;
using TechTalk.SpecFlow;

using Zilon.Core.Spec.Contexts;

namespace Zilon.Core.Spec.Steps
{
    [Binding]
    public class SurvivalSteps
    {
        private readonly SurvivalContext _context;

        public SurvivalSteps(SurvivalContext context)
        {
            _context = context;
        }

        [Given(@"Есть произвольная карта")]
        public void GivenЕстьПроизвольнаяКарта()
        {
            _context.CreateSector();
        }

        [Given(@"Есть персонаж игрока")]
        public void GivenЕстьПерсонажИгрока()
        {
            _context.AddHumanActor(new OffsetCoords(0, 0));
        }

        [Given(@"В инвентаре у актёра есть еда: (.*) количество: (.*)")]
        public void GivenВИнвентареУАктёраЕстьЕдаСыр(string propSid, int count)
        {
            var actor = _context.GetActiveActor();
            _context.AddResourceToActor(propSid, count, actor);
        }

        [When(@"Я перемещаю персонажа на одну клетку")]
        public void WhenЯПеремещаюПерсонажаНаОднуКлетку()
        {
            _context.MoveOnceActiveActor(new OffsetCoords(1, 0));
        }

        [When(@"Актёр съедает еду: (.*)")]
        public void WhenАктёрСъедаетЕду(string propSid)
        {
            _context.UsePropByActiveActor(propSid);
        }

        [Then(@"Значение сытости уменьшилось на (.*) единицу и стало (.*)")]
        public void ThenЗначениеСытостиУменьшилосьНаЕдиницу(int hungerRate, int expectedSatiety)
        {
            var actor = _context.GetActiveActor();
            actor.Person.Survival.Satiety.Should().Be(expectedSatiety);
        }

        [Then(@"Значение воды уменьшилось на (.*) единицу и стало (.*)")]
        public void ThenЗначениеВодыУменьшилосьНаЕдиницу(int thirstRate, int expectedThirst)
        {
            var actor = _context.GetActiveActor();
            actor.Person.Survival.Thirst.Should().Be(expectedThirst);
        }

        [Then(@"Значение сытости повысилось на (.*) единиц и уменьшилось на (.*) из-за голода")]
        public void ThenЗначениеСытостиПовысилосьНаЕдиниц(int satietyValue, int hungerRate)
        {
            ScenarioContext.Current.Pending();
        }

        [Then(@"Еда (.*) отсутствует в инвентаре персонажа")]
        public void ThenЕдаСырОтсутствуетВИнвентареПерсонажа(string propSid)
        {
            ScenarioContext.Current.Pending();
        }

    }
}
