using BlazorGoogleLogin.Shared.Models.future.toAdd;
using BlazorGoogleLogin.Shared.Models.future.toEdit;
using BlazorGoogleLogin.Shared.Models.future.toShow;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using BlazorGoogleLogin.Data;
using BlazorGoogleLogin.Shared.Models.present.toShow;

namespace BlazorGoogleLogin.Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class FutureController : ControllerBase
    {
        private readonly DbRepository _db;

        public FutureController(DbRepository db)
        {
            _db = db;
        }

        [HttpGet("userFutureGoalsToShow/{userID}")] // שליפה של היעדים בסביבה השלישית לפני לחיצות על כפתורים
        public async Task<IActionResult> GetUserFutureGoals(int userID)
        {
           
            var userGoalsQuery = "SELECT * FROM goals WHERE userID = @ID";
           
            
            var userGoals = (await _db.GetRecordsAsync<AllUserGoalsToShow>(userGoalsQuery, new { ID = userID })).ToList();
            if (userGoals.Count > 0)
            {
                return Ok(userGoals);
                
            }
            else
            {
                return BadRequest("goals not found");
            }

        }

        [HttpGet("userFutureSavingsToShow/{userID}")] // שליפה של החסכונות בסביבה השלישית לפני לחיצות על כפתורים
        public async Task<IActionResult> GetUserFutureSavings(int userID)
        {

            var userSavingssQuery = "SELECT * FROM savings WHERE userID = @ID";


            var userSavings = (await _db.GetRecordsAsync<AllUserSavingsToShow>(userSavingssQuery, new { ID = userID })).ToList();
            if (userSavings.Count > 0)
            {
                return Ok(userSavings);

            }
            else
            {
                return BadRequest("savings not found");
            }

        }

        [HttpPost("addGoal/{userID}")] // הוספת יעד חדש
        public async Task<IActionResult> addGoal(int userID, GoalToAdd UsergoalToAdd)
        {
            object newGoalParam = new
            {
                goalTitle = UsergoalToAdd.goalTitle,
                budget = UsergoalToAdd.budget,
                dueYear = UsergoalToAdd.dueYear,
                color = UsergoalToAdd.color,
                userID = userID
            };
            string insertGoalQuery = "insert into goals(goalTitle, budget, dueYear, color, userID) values (@goalTitle, @budget, @dueYear, @color, @userID)";

            int wasGoalAdded = await _db.InsertReturnIdAsync(insertGoalQuery, newGoalParam);
            if (wasGoalAdded > 0)
            {
                string GoalQuery = "SELECT * FROM goals Where id = @wasGoalAdded";
                var newGoalAfterAdding = await _db.GetRecordsAsync<AllUserGoalsToShow>(GoalQuery, wasGoalAdded);
                AllUserGoalsToShow newGoalToShow = newGoalAfterAdding.FirstOrDefault();
                return Ok(newGoalToShow);
            }
            return BadRequest("failed to add goal");
        }

        [HttpPost("addSaving/{userID}")] // הוספת חסכון חדש
        public async Task<IActionResult> addSaving(int userID, SavingToAdd UserSavingToAdd)
        {
            object newSavingParam = new
            {
                savingTitle = UserSavingToAdd.savingsTitle,
                savingsSum = UserSavingToAdd.savingsSum,
                interestRate = 0.4,
                userID = userID
            };
            string insertSavingQuery = "insert into savings(savingsTitle, savingsSum, interestRate, userID) values (@savingTitle, @savingsSum,@interestRate, @userID)";

            int wasSavingAdded = await _db.InsertReturnIdAsync(insertSavingQuery, newSavingParam);
            if (wasSavingAdded > 0)
            {

                var id = new { id = wasSavingAdded };  

                string SavingQuery = "SELECT * FROM savings Where id = @id";
                var newSavingAfterAdding = await _db.GetRecordsAsync<AllUserSavingsToShow>(SavingQuery, id);
                AllUserSavingsToShow newSavingToShow = newSavingAfterAdding.FirstOrDefault();
                return Ok(newSavingToShow);
            }
            return BadRequest("failed to add saving");
        }

















    }
}