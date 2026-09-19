using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EcoStudy.Models;

namespace EcoStudy.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created and up to date
            await context.Database.MigrateAsync();

            // 1. Seed Roles
            string[] roles = { "Teacher", "Student" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Default Teacher
            string teacherEmail = "teacher@ecostudy.com";
            var teacher = await userManager.FindByEmailAsync(teacherEmail);
            if (teacher == null)
            {
                teacher = new ApplicationUser
                {
                    UserName = teacherEmail,
                    Email = teacherEmail,
                    EmailConfirmed = true,
                    FullName = "Prof. Robert Anderson",
                    UserRole = "Teacher",
                    GradeOrLevel = "Senior Economics Faculty",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(teacher, "Teacher@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacher, "Teacher");
                }
            }

            // 3. Seed Default Student
            string studentEmail = "student@ecostudy.com";
            var student = await userManager.FindByEmailAsync(studentEmail);
            if (student == null)
            {
                student = new ApplicationUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    EmailConfirmed = true,
                    FullName = "Alex Morgan",
                    UserRole = "Student",
                    GradeOrLevel = "Grade 12 Economics",
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(student, "Student@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(student, "Student");
                }
            }

            // 4. Seed Additional Student for Teacher's student directory
            string student2Email = "sarah.jenkins@ecostudy.com";
            var student2 = await userManager.FindByEmailAsync(student2Email);
            if (student2 == null)
            {
                student2 = new ApplicationUser
                {
                    UserName = student2Email,
                    Email = student2Email,
                    EmailConfirmed = true,
                    FullName = "Sarah Jenkins",
                    UserRole = "Student",
                    GradeOrLevel = "Grade 11 Economics",
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                };
                var res2 = await userManager.CreateAsync(student2, "Student@123");
                if (res2.Succeeded)
                {
                    await userManager.AddToRoleAsync(student2, "Student");
                }
            }

            // 5. Seed Economics Chapters, Notes, and Important Questions
            if (!context.Chapters.Any())
            {
                var ch1 = new Chapter
                {
                    ChapterNumber = 1,
                    Name = "Introduction to Economics",
                    Description = "Explores central problems of an economy, Production Possibility Frontier (PPF), and positive vs normative economics.",
                    SubjectArea = "Microeconomics",
                    CreatedAt = DateTime.UtcNow
                };

                var ch2 = new Chapter
                {
                    ChapterNumber = 2,
                    Name = "Demand and Supply",
                    Description = "Covers utility analysis, cardinal vs ordinal approaches, indifference curves, and market demand and supply curves.",
                    SubjectArea = "Microeconomics",
                    CreatedAt = DateTime.UtcNow
                };

                var ch3 = new Chapter
                {
                    ChapterNumber = 3,
                    Name = "Theory of Production",
                    Description = "Examines production functions, short-run vs long-run, Law of Variable Proportions, and cost curves.",
                    SubjectArea = "Microeconomics",
                    CreatedAt = DateTime.UtcNow
                };

                var ch4 = new Chapter
                {
                    ChapterNumber = 4,
                    Name = "National Income Accounting",
                    Description = "Detailed analysis of GDP, GNP, NNP at factor cost and market prices, circular flow of income, and calculation methods.",
                    SubjectArea = "Macroeconomics",
                    CreatedAt = DateTime.UtcNow
                };

                var ch5 = new Chapter
                {
                    ChapterNumber = 5,
                    Name = "Money and Banking",
                    Description = "Functions of money, credit creation by commercial banks, and monetary policy tools of the Central Bank.",
                    SubjectArea = "Macroeconomics",
                    CreatedAt = DateTime.UtcNow
                };

                context.Chapters.AddRange(ch1, ch2, ch3, ch4, ch5);
                await context.SaveChangesAsync();

                // Ensure sample uploads directory exists
                string uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "notes");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Helper to create sample file
                void CreateSampleDoc(string fileName, string textContent)
                {
                    string p = Path.Combine(uploadsPath, fileName);
                    if (!File.Exists(p))
                    {
                        File.WriteAllText(p, textContent);
                    }
                }

                CreateSampleDoc("ch1_ppc_handout.pdf", "EcoStudy Notes: Chapter 1 Production Possibility Curve (PPC) Summary.\nOpportunity Cost and MRT.\nFaculty: Prof. Robert Anderson");
                CreateSampleDoc("ch2_indifference_curves.docx", "EcoStudy Notes: Chapter 2 Theory of Consumer Behaviour.\nIndifference Curve Properties and Consumer Equilibrium.\nFaculty: Prof. Robert Anderson");
                CreateSampleDoc("ch3_production_cost.pdf", "EcoStudy Notes: Chapter 3 Law of Variable Proportions.\nStage I, Stage II, Stage III Analysis.");
                CreateSampleDoc("ch4_national_income.pptx", "EcoStudy Slides: Chapter 4 National Income Accounting.\nGDP, GNP, NNP and Value Added Method.");
                CreateSampleDoc("ch5_money_banking.pdf", "EcoStudy Notes: Chapter 5 Commercial Banking & Central Bank Monetary Tools.");

                // Seed Notes for Chapters
                var notes = new List<Note>
                {
                    new Note
                    {
                        Title = "Core Concepts: Production Possibility Curve (PPC)",
                        Description = "Concavity to the origin, opportunity cost principle, and causes of PPC shifts.",
                        FileName = "PPC_Core_Concepts.pdf",
                        FilePath = "uploads/notes/ch1_ppc_handout.pdf",
                        FileType = ".pdf",
                        FileSize = 1048576, // 1 MB
                        ChapterId = ch1.Id,
                        TeacherId = teacher?.Id,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-10)
                    },
                    new Note
                    {
                        Title = "Indifference Curve Analysis & Properties",
                        Description = "Four core properties of IC curves and the condition for consumer equilibrium (MRSxy = Px/Py).",
                        FileName = "Indifference_Curves_Guide.docx",
                        FilePath = "uploads/notes/ch2_indifference_curves.docx",
                        FileType = ".docx",
                        FileSize = 524288, // 512 KB
                        ChapterId = ch2.Id,
                        TeacherId = teacher?.Id,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-7)
                    },
                    new Note
                    {
                        Title = "Law of Variable Proportions (Short-run Production)",
                        Description = "Understanding Stage 1 (Increasing Returns), Stage 2 (Diminishing Returns), and Stage 3 (Negative Returns).",
                        FileName = "Variable_Proportions_Stage_Analysis.pdf",
                        FilePath = "uploads/notes/ch3_production_cost.pdf",
                        FileType = ".pdf",
                        FileSize = 786432, // 768 KB
                        ChapterId = ch3.Id,
                        TeacherId = teacher?.Id,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-4)
                    },
                    new Note
                    {
                        Title = "Circular Flow of Income & National Income Aggregates",
                        Description = "Two-sector model, leakages vs injections, and conversions between Gross/Net, Domestic/National, Market Price/Factor Cost.",
                        FileName = "National_Income_Aggregates_Slides.pptx",
                        FilePath = "uploads/notes/ch4_national_income.pptx",
                        FileType = ".pptx",
                        FileSize = 2097152, // 2 MB
                        ChapterId = ch4.Id,
                        TeacherId = teacher?.Id,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-2)
                    },
                    new Note
                    {
                        Title = "Credit Creation Process by Commercial Banks",
                        Description = "Money multiplier formula (1/LRR) and balance sheet demonstration of fractional reserve banking.",
                        FileName = "Credit_Creation_Banking.pdf",
                        FilePath = "uploads/notes/ch5_money_banking.pdf",
                        FileType = ".pdf",
                        FileSize = 943718, // 920 KB
                        ChapterId = ch5.Id,
                        TeacherId = teacher?.Id,
                        IsPublished = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-1)
                    }
                };

                context.Notes.AddRange(notes);

                // Seed Important Questions (Short, Long, Numerical)
                var questions = GetDefaultQuestions(ch1, ch2, ch3, ch4, ch5);
                context.ImportantQuestions.AddRange(questions);
                await context.SaveChangesAsync();
            }
            else
            {
                // Ensure existing database gets Short, Long, and Numerical questions
                if (!await context.ImportantQuestions.AnyAsync(q => q.QuestionType == "Numerical"))
                {
                    var existingChapters = await context.Chapters.OrderBy(c => c.ChapterNumber).ToListAsync();
                    if (existingChapters.Count >= 5)
                    {
                        context.ImportantQuestions.RemoveRange(context.ImportantQuestions);
                        await context.SaveChangesAsync();

                        var questions = GetDefaultQuestions(existingChapters[0], existingChapters[1], existingChapters[2], existingChapters[3], existingChapters[4]);
                        context.ImportantQuestions.AddRange(questions);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        private static List<ImportantQuestion> GetDefaultQuestions(Chapter ch1, Chapter ch2, Chapter ch3, Chapter ch4, Chapter ch5)
        {
            return new List<ImportantQuestion>
            {
                // Chapter 1: Introduction to Economics
                new ImportantQuestion
                {
                    Question = "Define Marginal Opportunity Cost (MOC) and state how it determines the shape of the PPC.",
                    QuestionType = "Short",
                    Marks = 2,
                    DifficultyLevel = "Easy",
                    ExamReference = "Board Exam 2022",
                    AnswerHint = "MOC is the ratio of units of one good sacrificed to produce an additional unit of another good. Increasing MOC makes the PPC concave to the origin.",
                    ChapterId = ch1.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new ImportantQuestion
                {
                    Question = "Explain why a Production Possibility Curve is concave to the origin. Illustrate with a schedule and diagram.",
                    QuestionType = "Long",
                    Marks = 6,
                    DifficultyLevel = "Medium",
                    ExamReference = "Board Exam 2023, 2024",
                    AnswerHint = "Define MRT = Delta Y / Delta X. Explain increasing marginal opportunity cost due to imperfect factor adaptability. Draw concave curve.",
                    ChapterId = ch1.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new ImportantQuestion
                {
                    Question = "A country produces Goods X and Y. Production combinations are: A(0, 30), B(1, 27), C(2, 21), D(3, 12), E(4, 0). Calculate Marginal Rate of Transformation (MRT) for each successive combination.",
                    QuestionType = "Numerical",
                    Marks = 4,
                    DifficultyLevel = "Medium",
                    ExamReference = "Model Paper 2023",
                    AnswerHint = "MRT = Delta Y / Delta X. A->B: 3/1 = 3; B->C: 6/1 = 6; C->D: 9/1 = 9; D->E: 12/1 = 12. Shows increasing opportunity cost.",
                    ChapterId = ch1.Id,
                    CreatedAt = DateTime.UtcNow
                },

                // Chapter 2: Theory of Consumer Behavior
                new ImportantQuestion
                {
                    Question = "What is an Indifference Map? State two key properties of indifference curves.",
                    QuestionType = "Short",
                    Marks = 3,
                    DifficultyLevel = "Easy",
                    ExamReference = "Annual Paper 2022",
                    AnswerHint = "An Indifference Map is a family of indifference curves representing different levels of satisfaction. Properties: 1) Downward sloping, 2) Convex to origin, 3) Never intersect.",
                    ChapterId = ch2.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new ImportantQuestion
                {
                    Question = "Explain consumer equilibrium using Indifference Curve technique with a neat diagram and necessary conditions.",
                    QuestionType = "Long",
                    Marks = 6,
                    DifficultyLevel = "Hard",
                    ExamReference = "Model Exam 2024",
                    AnswerHint = "Conditions: 1) MRSxy = Px/Py (Budget line tangent to IC), 2) IC must be strictly convex at point of equilibrium. Draw diagram showing tangent point E.",
                    ChapterId = ch2.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new ImportantQuestion
                {
                    Question = "A consumer buys 50 units of a good at Rs 10 per unit. When price drops to Rs 8 per unit, demand increases to 70 units. Calculate Price Elasticity of Demand (Ed) and comment on its elasticity.",
                    QuestionType = "Numerical",
                    Marks = 4,
                    DifficultyLevel = "Medium",
                    ExamReference = "Board Exam 2023",
                    AnswerHint = "Ed = (Delta Q / Delta P) * (P / Q) = (20 / -2) * (10 / 50) = -10 * 0.2 = -2. Elasticity magnitude is 2 (> 1), indicating highly price elastic demand.",
                    ChapterId = ch2.Id,
                    CreatedAt = DateTime.UtcNow
                },

                // Chapter 3: Production and Costs
                new ImportantQuestion
                {
                    Question = "Differentiate between Explicit Cost and Implicit Cost with suitable economic examples.",
                    QuestionType = "Short",
                    Marks = 2,
                    DifficultyLevel = "Easy",
                    ExamReference = "Sample Paper 2023",
                    AnswerHint = "Explicit costs are out-of-pocket payments to factor owners (e.g. wages, raw materials). Implicit costs are imputed values of self-owned resources (e.g. rent of self-owned premises).",
                    ChapterId = ch3.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new ImportantQuestion
                {
                    Question = "State and explain the Law of Variable Proportions. Why does a rational producer always operate in Stage II?",
                    QuestionType = "Long",
                    Marks = 6,
                    DifficultyLevel = "Hard",
                    ExamReference = "Board Exam 2021, 2023",
                    AnswerHint = "Explain the three stages: 1) Increasing returns, 2) Diminishing returns, 3) Negative returns. In Stage I, fixed factors are underutilized; in Stage III MP is negative. Stage II is the economic zone.",
                    ChapterId = ch3.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new ImportantQuestion
                {
                    Question = "A firm's Total Fixed Cost (TFC) is Rs 120. Total Variable Cost (TVC) at 1, 2, 3, 4 units of output is Rs 40, 70, 110, 170 respectively. Calculate Average Variable Cost (AVC) and Marginal Cost (MC) at each output.",
                    QuestionType = "Numerical",
                    Marks = 5,
                    DifficultyLevel = "Medium",
                    ExamReference = "Board Exam 2024",
                    AnswerHint = "Unit 1: AVC=40, MC=40. Unit 2: AVC=35, MC=30. Unit 3: AVC=36.67, MC=40. Unit 4: AVC=42.5, MC=60. Show tabular schedule.",
                    ChapterId = ch3.Id,
                    CreatedAt = DateTime.UtcNow
                },

                // Chapter 4: National Income Accounting
                new ImportantQuestion
                {
                    Question = "Distinguish between Factor Income and Transfer Payment with one example each.",
                    QuestionType = "Short",
                    Marks = 2,
                    DifficultyLevel = "Easy",
                    ExamReference = "Annual Paper 2021",
                    AnswerHint = "Factor income is earned for contributing productive services (wages, rent, interest, profit). Transfer payment is unearned unilateral receipt (scholarships, pensions) and excluded from National Income.",
                    ChapterId = ch4.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new ImportantQuestion
                {
                    Question = "What precautions must be observed while estimating National Income using the Value Added Method and Expenditure Method?",
                    QuestionType = "Long",
                    Marks = 6,
                    DifficultyLevel = "Medium",
                    ExamReference = "Board Exam 2023",
                    AnswerHint = "Value Added: exclude intermediate consumption, exclude 2nd hand goods, include self-consumption & imputed rent. Expenditure: exclude 2nd hand goods, exclude transfer payments, include net exports.",
                    ChapterId = ch4.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new ImportantQuestion
                {
                    Question = "Calculate GDP at Market Price and National Income (NNP at FC) from the following data (Rs in Crores): Private Final Consumption = 900, Govt Final Consumption = 400, Gross Domestic Fixed Capital Formation = 250, Change in Stock = 50, Net Exports = -40, Depreciation = 60, Net Indirect Tax = 100, NFIA = -20.",
                    QuestionType = "Numerical",
                    Marks = 6,
                    DifficultyLevel = "Hard",
                    ExamReference = "Board Exam 2024",
                    AnswerHint = "GDP_MP = C + G + I + (X-M) = 900 + 400 + (250+50) + (-40) = 1560 Crores. NNP_FC = GDP_MP - Dep + NFIA - NIT = 1560 - 60 + (-20) - 100 = 1380 Crores.",
                    ChapterId = ch4.Id,
                    CreatedAt = DateTime.UtcNow
                },

                // Chapter 5: Money and Banking
                new ImportantQuestion
                {
                    Question = "What is the difference between Central Bank and Commercial Banks? Give two points.",
                    QuestionType = "Short",
                    Marks = 2,
                    DifficultyLevel = "Easy",
                    ExamReference = "Board Exam 2022",
                    AnswerHint = "Central Bank is apex monetary authority, issues currency notes, operates for public interest. Commercial banks accept public deposits, create credit, operate for profit.",
                    ChapterId = ch5.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new ImportantQuestion
                {
                    Question = "Explain the credit control functions of the Central Bank using Bank Rate, Open Market Operations, and Reverse Repo Rate.",
                    QuestionType = "Long",
                    Marks = 6,
                    DifficultyLevel = "Medium",
                    ExamReference = "Sample Paper 2024",
                    AnswerHint = "Explain how increasing bank rate or selling govt securities in open market absorbs liquidity, contracting credit creation during demand-pull inflation.",
                    ChapterId = ch5.Id,
                    CreatedAt = DateTime.UtcNow
                },
                new ImportantQuestion
                {
                    Question = "Suppose Legal Reserve Ratio (LRR) is 10% and an initial cash deposit of Rs 1,000 crores is made into commercial banks. Calculate the Money Multiplier and total deposit creation.",
                    QuestionType = "Numerical",
                    Marks = 4,
                    DifficultyLevel = "Easy",
                    ExamReference = "Model Exam 2023",
                    AnswerHint = "Money Multiplier = 1 / LRR = 1 / 0.10 = 10. Total Credit / Deposit Created = Initial Deposit * Multiplier = 1,000 * 10 = Rs 10,000 Crores.",
                    ChapterId = ch5.Id,
                    CreatedAt = DateTime.UtcNow
                }
            };
        }
    }
}
