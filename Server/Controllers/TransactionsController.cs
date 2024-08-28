using BlazorGoogleLogin.Shared.Models.present.toAdd;
using BlazorGoogleLogin.Shared.Models.present.toEdit;
using BlazorGoogleLogin.Shared.Models.present.toShow;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BlazorGoogleLogin.Data;
using Blazorise;

namespace BlazorGoogleLogin.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly DbRepository _db;

        public TransactionsController(DbRepository db)
        {
            _db = db;
        }

        [HttpPost("AddTransaction")] // יצירת הזנה חדשה
        public async Task<IActionResult> AddTransaction(TransactionToAdd TransactionToAdd)
        {
            if (TransactionToAdd.splitPayment == null)
            {
                TransactionToAdd.splitPayment = false;
            }

            object TransToAddParam = new
            {
                transTitle = TransactionToAdd.transTitle,
                subCategoryID = TransactionToAdd.subCategoryID,
                transType = TransactionToAdd.transType,
                transValue = TransactionToAdd.transValue,
                valueType = TransactionToAdd.valueType,
                transDate = TransactionToAdd.transDate,
                description = TransactionToAdd.description,
                fixedMonthly = TransactionToAdd.fixedMonthly,
                parentTransID = TransactionToAdd.parentTransID,
                tagID = TransactionToAdd.tagID,
                splitPayment = TransactionToAdd.splitPayment
            };


            string insertTransQuery = "INSERT INTO transactions (transTitle,subCategoryID, transType, transValue, valueType, transDate, description, fixedMonthly, parentTransID, tagID, splitPayment) values (@transTitle,@subCategoryID, @transType, @transValue, @valueType, @transDate, @description, @fixedMonthly, @parentTransID, @tagID, @splitPayment)";

            int newTransId = await _db.InsertReturnIdAsync(insertTransQuery, TransToAddParam);

            if (newTransId != 0)
            {
                return Ok(newTransId);
            }

            return BadRequest("Transaction not created");
        }

        [HttpGet("showOverdraft/{subCatID}/{userID}")]
        public async Task<IActionResult> showOverdraft(int subCatID, int userID)
        {
            List<OverBudgetToShow> subcategoriesToCloseGap = new List<OverBudgetToShow>();
            List<OverBudgetToShow> subcategoriesToCloseGapAfterLoop = new List<OverBudgetToShow>();
            OverBudgetToShow currentOverdraft = new OverBudgetToShow();

            object param = new
            {
                ID = subCatID
            };

            string GetCategoryCurrentSumQuery = "SELECT COALESCE(SUM(t.transValue), 0)  FROM transactions t JOIN subcategories sc ON sc.id = t.subCategoryID JOIN categories c ON c.id = sc.categoryID JOIN users u ON u.id = c.userID WHERE sc.id=@ID AND (t.transType = 1 OR t.transType = 3) AND t.transDate >= DATE_FORMAT(CASE WHEN DAY(CURRENT_DATE) >= u.monthStartDate THEN CONCAT(YEAR(CURRENT_DATE), '-', LPAD(MONTH(CURRENT_DATE), 2, '0'), '-', LPAD(u.monthStartDate, 2, '0')) ELSE CONCAT(YEAR(CURRENT_DATE - INTERVAL 1 MONTH), '-', LPAD(MONTH(CURRENT_DATE - INTERVAL 1 MONTH), 2, '0'), '-', LPAD(u.monthStartDate, 2, '0')) END, '%Y-%m-%d') AND YEAR(t.transDate) = YEAR(CURRENT_DATE()) AND MONTH(t.transDate) = MONTH(CURRENT_DATE());";
            var recordSubCatCurrentSum = await _db.GetRecordsAsync<double>(GetCategoryCurrentSumQuery, param);
            if (recordSubCatCurrentSum != null)
            {
                string GetSubCategoryBudgetQuery = "SELECT monthlyPlannedBudget FROM subcategories WHERE id = @ID";
                var budgetRes = await _db.GetRecordsAsync<int>(GetSubCategoryBudgetQuery, param);
                if (budgetRes != null)
                {
                    string GetSubCategoryNameQuery = "SELECT subCategoryTitle FROM subcategories WHERE id = @ID";
                    var recordGetSubCategoryName = await _db.GetRecordsAsync<string>(GetSubCategoryNameQuery, param);
                    if (recordGetSubCategoryName != null)
                    {
                        //currentOverdraft is different than the other list items, its remaining budget is the sum of all its expenses, when in the other list items it is the budget free to close the gap
                        currentOverdraft.subCategoryTitle = recordGetSubCategoryName.FirstOrDefault();
                        currentOverdraft.monthlyPlannedBudget = budgetRes.FirstOrDefault();
                        currentOverdraft.remainingBudget = recordSubCatCurrentSum.FirstOrDefault();
                        currentOverdraft.id = subCatID;

                        if (currentOverdraft.monthlyPlannedBudget >= currentOverdraft.remainingBudget)
                        {
                            return BadRequest("אין חריגה");
                        }

                        double gap = (currentOverdraft.remainingBudget - currentOverdraft.monthlyPlannedBudget);
                        Console.WriteLine("the budget gap is- " + gap);


                        object userCatParam = new
                        {
                            userID = userID
                        };

                        string getUserCategories = "SELECT id FROM categories where userID = @userID";
                        var recordUserCategories = await _db.GetRecordsAsync<int>(getUserCategories, userCatParam);
                        List<int> userCatesList = recordUserCategories.ToList();

                        if (userCatesList.Count > 0)
                        {
                            foreach (var category in userCatesList)
                            {
                                if (category != null && category > 0)
                                {

                                    object gapParam = new
                                    {
                                        gap = gap,
                                        categoryID = category
                                    };

                                    string getFittingSubCats = "SELECT subcategories.id,subcategories.subCategoryTitle,    subcategories.monthlyPlannedBudget,(subcategories.monthlyPlannedBudget - COALESCE(SUM(transactions.transValue), 0)) AS remainingBudget FROM subcategories LEFT JOIN   transactions ON subcategories.id = transactions.subCategoryID AND MONTH(transactions.transDate) = MONTH(CURRENT_DATE()) AND YEAR(transactions.transDate) = YEAR(CURRENT_DATE()) WHERE subcategories.importance = 0 AND subcategories.categoryID = @categoryID GROUP BY subcategories.id, subcategories.subCategoryTitle, subcategories.monthlyPlannedBudget HAVING remainingBudget > @gap;";

                                    var optionalSubcategories = await _db.GetRecordsAsync<OverBudgetToShow>(getFittingSubCats, gapParam);

                                    if (optionalSubcategories != null)
                                    {
                                        subcategoriesToCloseGap = optionalSubcategories.ToList();

                                        foreach (OverBudgetToShow subCatToShow in subcategoriesToCloseGap)
                                        {
                                            subcategoriesToCloseGapAfterLoop.Add(subCatToShow);
                                        }
                                    }
                                }
                            }
                            subcategoriesToCloseGapAfterLoop.Add(currentOverdraft);
                            return Ok(subcategoriesToCloseGapAfterLoop);
                        }
                        return BadRequest("no category found");
                    }
                    return BadRequest("no sub category found");
                }
                return BadRequest("no linked transactions found to this subcategory");
            }
            return BadRequest("couldnt find sum");
        }

        [HttpPost("EditSubCategoriesNewBudgets")]  // עריכת תקציב חדש לאחר העברה בחריגה

        public async Task<IActionResult> EditSubCategoriesNewBudgets([FromBody] List<OverDraftBudgetToEdit> budgetToUpdate)
        {

            bool isBudgetUpdate = false;

            foreach (var newBudget in budgetToUpdate)
            {

                object updateBudgetParam = new
                {
                    ID = newBudget.id,
                    monthlyPlannedBudget = newBudget.monthlyPlannedBudget
                };

                string UpdateSubCategoryBudgetQuery = "UPDATE subcategories set monthlyPlannedBudget = @monthlyPlannedBudget where id =@ID";
                isBudgetUpdate = await _db.SaveDataAsync(UpdateSubCategoryBudgetQuery, updateBudgetParam);

            }

            if (isBudgetUpdate)
            {
                return Ok("התקציב עודכן בהצלחה");
            }
            return BadRequest("update sub category budget failed");


        }

        [HttpGet("updateGivingSubCat/{subCatID}")]
        public async Task<IActionResult> updateGivingSubCat(int subCatID)
        {
            object param = new
            {
                ID = subCatID
            };

            string GetSubCatCurrentBudgetQuery = "SELECT monthlyPlannedBudget FROM subcategories WHERE id = @ID";
            var recordSubCatCurrentSum = await _db.GetRecordsAsync<double>(GetSubCatCurrentBudgetQuery, param);
            double newBudget = recordSubCatCurrentSum.FirstOrDefault();
            if (newBudget != null)
            {
                return Ok(newBudget);
            }

            return BadRequest("Couldn't find this sub cat's budget");
        }

        //        [HttpGet("getAllTransactions/{subCatID}")]
        //        public async Task<IActionResult> getAllTransactions(int userID, int subCatID)
        //        {

        //            object param = new
        //            {
        //                ID = subCatID,
        //                StartOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
        //                EndOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month))
        //            };

        //            string getAllSubCatTransactionsQuery = @"
        //        SELECT 
        //    transactions.id, 
        //    transactions.transType, 
        //    transactions.transValue, 
        //    transactions.valueType, 
        //    DATE_FORMAT(transactions.transDate, '%d/%m/%Y') AS transDate, 
        //    transactions.description, 
        //    transactions.fixedMonthly, 
        //    transactions.tagID, 
        //    transactions.transTitle, 
        //    transactions.parentTransID,
        //    transactions.splitPayment,
        //    tags.tagTitle,
        //    tags.tagColor
        //FROM transactions 
        //LEFT JOIN tags ON transactions.tagID = tags.id
        //WHERE transactions.subCategoryID = @ID
        //AND transactions.transDate >= @StartOfMonth
        //AND transactions.transDate <= @EndOfMonth
        //AND (transactions.transType = 1 OR transactions.transType = 3)
        //ORDER BY transactions.transDate DESC;";

        //            var recordSubCatCurrentTrans = await _db.GetRecordsAsync<TransactionOverviewToShow>(getAllSubCatTransactionsQuery, param);
        //            var subCatTransactions = recordSubCatCurrentTrans.ToList();

        //            if (subCatTransactions != null)
        //            {
        //                return Ok(subCatTransactions);
        //            }

        //            return BadRequest("Couldn't find this sub cat's transactions");
        //        }

        [HttpGet("getAllTransactionsByDate/{userID}/{subCatID}")]
        public async Task<IActionResult> getAllTransactionsByDate(int userID, int subCatID)
        {

            object param = new
            {
                ID = userID,
                subCategoryID = subCatID,
            };

            string getAllSubCatTransactionsQuery = @"
SELECT transactions.id, transactions.transType, transactions.transValue, transactions.valueType, DATE_FORMAT(transactions.transDate, '%d/%m/%Y') AS transDate, transactions.description, transactions.fixedMonthly, transactions.tagID, transactions.transTitle, transactions.parentTransID, transactions.splitPayment, tags.tagTitle, tags.tagColor
FROM transactions 
LEFT JOIN tags ON transactions.tagID = tags.id 
WHERE transactions.subCategoryID = @subCategoryID 
AND transactions.transDate >= (
    SELECT 
        CASE 
            WHEN DAY(CURRENT_DATE()) >= u.monthStartDate 
            THEN CONCAT(YEAR(CURRENT_DATE()), '-', LPAD(MONTH(CURRENT_DATE()), 2, '0'), '-', LPAD(u.monthStartDate, 2, '0')) 
            ELSE CONCAT(YEAR(CURRENT_DATE() - INTERVAL 1 MONTH), '-', LPAD(MONTH(CURRENT_DATE() - INTERVAL 1 MONTH), 2, '0'), '-', LPAD(u.monthStartDate, 2, '0')) 
        END 
    AS StartOfMonth 
    FROM users u 
    WHERE u.id = @ID
)
AND transactions.transDate <= CURRENT_DATE() 
AND (transactions.transType = 1 OR transactions.transType = 3) 
ORDER BY transactions.transDate DESC;";

            var recordSubCatCurrentTrans = await _db.GetRecordsAsync<TransactionOverviewToShow>(getAllSubCatTransactionsQuery, param);
            var subCatTransactions = recordSubCatCurrentTrans.ToList();

            if (subCatTransactions != null)
            {
                return Ok(subCatTransactions);
            }

            return BadRequest("Couldn't find this sub cat's transactions");
        }

        [HttpGet("getAllIncomeTransactions/{userID}/{subCatID}")]
        public async Task<IActionResult> getAllIncomeTransactions(int userID, int subCatID)
        {
            object param = new
            {
                ID = userID,
                subCategoryID = subCatID,
            };

            string getAllSubCatTransactionsQuery = @"SELECT transactions.id, transactions.transType, transactions.transValue, transactions.valueType, DATE_FORMAT(transactions.transDate, '%d/%m/%Y') AS transDate, transactions.description, transactions.fixedMonthly, transactions.tagID, transactions.transTitle, transactions.parentTransID, transactions.splitPayment, tags.tagTitle, tags.tagColor
FROM transactions 
LEFT JOIN tags ON transactions.tagID = tags.id 
WHERE transactions.subCategoryID = @subCategoryID 
AND transactions.transDate >= (
    SELECT 
        CASE 
            WHEN DAY(CURRENT_DATE()) >= u.monthStartDate 
            THEN CONCAT(YEAR(CURRENT_DATE()), '-', LPAD(MONTH(CURRENT_DATE()), 2, '0'), '-', LPAD(u.monthStartDate, 2, '0')) 
            ELSE CONCAT(YEAR(CURRENT_DATE() - INTERVAL 1 MONTH), '-', LPAD(MONTH(CURRENT_DATE() - INTERVAL 1 MONTH), 2, '0'), '-', LPAD(u.monthStartDate, 2, '0')) 
        END 
    AS StartOfMonth 
    FROM users u 
    WHERE u.id = @ID
)
AND transactions.transDate <= CURRENT_DATE() 
AND transactions.transType = 2
ORDER BY transactions.transDate DESC;";

            var recordSubCatCurrentTrans = await _db.GetRecordsAsync<TransactionOverviewToShow>(getAllSubCatTransactionsQuery, param);
            var subCatTransactions = recordSubCatCurrentTrans.ToList();

            if (subCatTransactions != null)
            {
                return Ok(subCatTransactions);
            }

            return BadRequest("Couldn't find this sub cat's transactions");
        }

        [HttpDelete("deleteTransaction/{TransIdToDelete}")] // מחיקת הזנה
        public async Task<IActionResult> DeleteTransaction(int TransIdToDelete)
        {
            string DeleteQuery = "DELETE FROM transactions WHERE id=@ID";
            bool isTransDeleted = await _db.SaveDataAsync(DeleteQuery, new { ID = TransIdToDelete });

            if (isTransDeleted)
            {
                return Ok();
            }

            return BadRequest("Failed to delete transaction");
        }


        [HttpPost("editTransaction")]
        public async Task<IActionResult> updateTransaction(TransactionToEdit transToEdit)
        {

            object transUpdateParam = new
            {
                ID = transToEdit.id,
                transType = transToEdit.transType,
                transValue = transToEdit.transValue,
                valueType = transToEdit.valueType,
                transDate = transToEdit.transDate,
                description = transToEdit.description,
                fixedMonthly = transToEdit.fixedMonthly,
                tagID = transToEdit.tagID,
                parentTransID = transToEdit.parentTransID,
                transTitle = transToEdit.transTitle,
                splitPayment = transToEdit.splitPayment
            };

            string UpdateTransQuery = "UPDATE transactions SET transType = @transType, transValue = @transValue, valueType = @valueType, transDate=@transDate, description=@description, fixedMonthly=@fixedMonthly, tagID=@tagID, parentTransID=@parentTransID, transTitle=@transTitle, splitPayment=@splitPayment WHERE id =@ID";
            bool isUpdate = await _db.SaveDataAsync(UpdateTransQuery, transUpdateParam);

            if (isUpdate)
            {
                return Ok(transToEdit);
            }
            return BadRequest("update transaction failed");
        }

        [HttpGet("updateOverDraftTrans/{transID}")]
        public async Task<IActionResult> updateOverDraftTrans(int transID)
        {
            object transTypeUpdateParam = new
            {
                ID = transID
            };

            string UpdateTransQuery = "UPDATE transactions set transType = 3 WHERE id =@ID";
            bool isUpdate = await _db.SaveDataAsync(UpdateTransQuery, transTypeUpdateParam);

            if (isUpdate)
            {
                return Ok(transID);
            }
            return BadRequest("update trans type failed");
        }


        [HttpGet("getAllUserTags/{userID}")]
        public async Task<IActionResult> getAllUserTags(int userID)
        {
            object allTagsParam = new
            {
                ID = userID
            };

            //string getTagsQuery = "SELECT DISTINCT tags.id, tags.tagTitle, tags.tagColor FROM users JOIN categories ON users.id = categories.userID JOIN subcategories ON categories.id = subcategories.categoryID JOIN transactions ON subcategories.id = transactions.subCategoryID JOIN tags ON transactions.tagID = tags.id WHERE users.id = @ID";

            string getTagsQuery = "SELECT distinct tags.id, tags.tagTitle, tags.tagColor from tags where userID = @ID";
            var recordUserTags = await _db.GetRecordsAsync<TagsToShow>(getTagsQuery, allTagsParam);
            List<TagsToShow> userTagsList = recordUserTags.ToList();

            if (userTagsList.Count > 0)
            {
                return Ok(userTagsList);
            }
            return BadRequest("tags not found");
        }

        [HttpGet("getSubCatTags/{subCatID}")]
        public async Task<IActionResult> getSubCatTags(int subCatID)
        {
            object param = new
            {
                ID = subCatID
            };

            string GetSubCatTagsQuery = "select distinct tagID from transactions where subCategoryID=@ID";
            var recordSubCatTags = await _db.GetRecordsAsync<int?>(GetSubCatTagsQuery, param);
            List<int?> TagsIDList = recordSubCatTags.ToList();
            if (TagsIDList.Count > 0)
            {
                List<TagsToShow> subCatTagList = new List<TagsToShow>();
                for (int i = 0; i < TagsIDList.Count; i++)
                {
                    if (TagsIDList[i] > 0)
                    {
                        object tagIdParam = new
                        {
                            ID = TagsIDList[i]
                        };
                        string getTagInfoQuery = "select * from tags where ID=@ID";
                        var getTagInfo = await _db.GetRecordsAsync<TagsToShow>(getTagInfoQuery, tagIdParam);
                        var subCatTag = getTagInfo.FirstOrDefault();
                        if (subCatTag != null)
                        {
                            subCatTagList.Add(subCatTag);
                        }
                    }
                }
                return Ok(subCatTagList);
            }

            return BadRequest("Couldn't find this sub cat's tags");
        }

        [HttpGet("getRepeatedTransToEdit/{ParentTransID}")]
        public async Task<IActionResult> getRepeatedTransToEdit(int ParentTransID)
        {
            object param = new
            {
                ID = ParentTransID
            };

            string getRepeatedTransToEditQuery = "SELECT id, transValue, transDate FROM transactions WHERE parentTransID = @ID";
            var recordRepeatedTransToEdit = await _db.GetRecordsAsync<RepeatedTransToShow>(getRepeatedTransToEditQuery, param);
            List<RepeatedTransToShow> repeatedTransValue = recordRepeatedTransToEdit.ToList();

            if (repeatedTransValue.Count > 0)
            {
                return Ok(repeatedTransValue);
            }

            return BadRequest("Couldn't find repeated trans values");
        }

        [HttpGet("getPaymentTransToEdit/{ParentTransID}")]
        public async Task<IActionResult> getPaymentTransToEdit(int ParentTransID)
        {
            object param = new
            {
                ID = ParentTransID
            };

            string getRepeatedTransToEditQuery = "SELECT id, transValue, transDate FROM transactions WHERE parentTransID = @ID OR id=@ID";
            var recordRepeatedTransToEdit = await _db.GetRecordsAsync<RepeatedTransToShow>(getRepeatedTransToEditQuery, param);
            List<RepeatedTransToShow> repeatedTransValue = recordRepeatedTransToEdit.ToList();

            if (repeatedTransValue.Count > 0)
            {
                return Ok(repeatedTransValue);
            }

            return BadRequest("Couldn't find repeated trans values");
        }

        [HttpPost("UpdateRepeatedTrans")]

        public async Task<IActionResult> UpdateRepeatedTrans(RepeatedTransToShow reTrans)
        {
            bool isTransUpdate = false;
            //[FromBody] List<RepeatedTransToShow> reTransToUpdate
            //foreach (var reTrans in reTransToUpdate)
            //{

            object updateTransParam = new
            {
                ID = reTrans.id,
                transValue = reTrans.transValue,
                transDate = reTrans.transDate,

            };

            string TransToUpdateQuery = "UPDATE transactions set transValue = @transValue, transDate=@transDate WHERE id =@ID";
            isTransUpdate = await _db.SaveDataAsync(TransToUpdateQuery, updateTransParam);

            //}

            if (isTransUpdate)
            {
                return Ok("הוצאה חוזרת עודכנה");
            }
            return BadRequest("עדכון הוצאה חוזרת נכשל");

        }

        [HttpDelete("deleteSplittedTransChildren/{TransIdToDelete}")] // מחיקת הזנה
        public async Task<IActionResult> deleteSplittedTransChildren(int TransIdToDelete)
        {
            string DeleteQuery = "DELETE FROM transactions WHERE parentTransID=@ID OR id=@ID";
            bool isTransDeleted = await _db.SaveDataAsync(DeleteQuery, new { ID = TransIdToDelete });

            if (isTransDeleted)
            {
                return Ok();
            }

            return BadRequest("Failed to delete transaction");
        }

        [HttpGet("identifyParent/{childTransID}")]
        public async Task<IActionResult> identifyParent(int childTransID)
        {
            object param = new
            {
                ID = childTransID
            };

            string findParentQuery = "SELECT id FROM transactions WHERE id=@ID AND splitPayment=1";
            var parentTransToEdit = await _db.GetRecordsAsync<int>(findParentQuery, param);
            int repeatedTransValue = parentTransToEdit.FirstOrDefault();

            if (repeatedTransValue > 0)
            {
                return Ok(repeatedTransValue);
            }

            return BadRequest("Parent doesn't have split payment");
        }

        [HttpGet("getSplitParent/{ParentTransID}")]
        public async Task<IActionResult> getSplitParent(int ParentTransID)
        {
            object param = new
            {
                ID = ParentTransID
            };

            string getRepeatedTransParentQuery = "SELECT * FROM transactions WHERE id=@ID";
            var recordSplitParent = await _db.GetRecordsAsync<TransactionToEdit>(getRepeatedTransParentQuery, param);
            TransactionToEdit repeatedTransValue = recordSplitParent.FirstOrDefault();

            if (repeatedTransValue != null)
            {
                return Ok(repeatedTransValue);
            }

            return BadRequest("Couldn't find repeated trans values");
        }

        [HttpDelete("deleteSplittedChildrenOnly/{TransIdToDelete}")] // מחיקת הזנה
        public async Task<IActionResult> deleteSplittedChildrenOnly(int TransIdToDelete)
        {
            string DeleteQuery = "DELETE FROM transactions WHERE parentTransID=@ID";
            bool isTransDeleted = await _db.SaveDataAsync(DeleteQuery, new { ID = TransIdToDelete });

            if (isTransDeleted)
            {
                return Ok();
            }

            return BadRequest("Failed to delete transaction");
        }

        [HttpPost("updateSplitChildren")]
        public async Task<IActionResult> updateSplitChildren(TransactionToEdit transToEdit)
        {

            object transUpdateParam = new
            {
                ID = transToEdit.id,
                transType = transToEdit.transType,
                transValue = transToEdit.transValue,
                valueType = transToEdit.valueType,

                description = transToEdit.description,
                fixedMonthly = transToEdit.fixedMonthly,
                tagID = transToEdit.tagID,
                parentTransID = transToEdit.parentTransID,
                transTitle = transToEdit.transTitle,
                splitPayment = transToEdit.splitPayment
            };

            string UpdateTransQuery = "UPDATE transactions SET transType = @transType, transValue = @transValue, valueType = @valueType,  description=@description, fixedMonthly=@fixedMonthly, tagID=@tagID, parentTransID=@parentTransID, transTitle=@transTitle, splitPayment=@splitPayment WHERE parentTransID =@ID";
            bool isUpdate = await _db.SaveDataAsync(UpdateTransQuery, transUpdateParam);

            if (isUpdate)
            {
                return Ok("updated all split children");
            }
            return BadRequest("failed to update children of split");
        }

        [HttpGet("getCatType/{userID}")] //only gets the subCat id if the category is income
        public async Task<IActionResult> getCatType(int userID)
        {
            object param = new
            {
                ID = userID
            };

            string getSubCatQuery = "SELECT sb.id FROM subcategories as sb join categories c on c.id=sb.categoryID join users u on c.userID=u.id where c.userID=u.id and c.categroyTitle=\"הכנסות\" and u.id=@ID";
            var getSubCatID = await _db.GetRecordsAsync<int>(getSubCatQuery, param);
            List<int> subCatIDs = getSubCatID.ToList();

            if (subCatIDs != null)
            {
                return Ok(subCatIDs);
            }

            return BadRequest("this transactions category isn't הכנסות");
        }

    }

}

