CREATE DATABASE NewsManagementDB
USE NewsManagementDB

/*
----------------------------------------------------------------
-- KỊCH BẢN TẠO CÁC BẢNG (SCHEMA)
-- Tạo các bảng theo thứ tự phụ thuộc:
-- 1. SystemAccount (Độc lập)
-- 2. Category (Có thể tự tham chiếu)
-- 3. Tag (Độc lập)
-- 4. NewsArticle (Phụ thuộc vào SystemAccount, Category)
-- 5. NewsTag (Phụ thuộc vào NewsArticle, Tag)
----------------------------------------------------------------
*/

-- 0. Xóa các bảng nếu tồn tại để chạy lại kịch bản
DROP TABLE IF EXISTS NewsTag;
DROP TABLE IF EXISTS NewsArticle;
DROP TABLE IF EXISTS Tag;
DROP TABLE IF EXISTS Category;
DROP TABLE IF EXISTS SystemAccount;
GO

-- 1. Bảng SystemAccount
CREATE TABLE SystemAccount (
    AccountID INT PRIMARY KEY IDENTITY(1,1),
    AccountName NVARCHAR(100) NOT NULL,
    AccountEmail VARCHAR(255) NOT NULL UNIQUE,
    AccountPassword NVARCHAR(255) NOT NULL, -- Trong thực tế, đây nên là mật khẩu đã được hash
    AccountRole INT NOT NULL -- Theo mô tả: 1=Staff, 2=Lecturer, (giả sử 0=Admin)
);
GO

-- 2. Bảng Category
CREATE TABLE Category (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL,
    CategoryDescription NVARCHAR(500),
    ParentCategoryID INT NULL,
    IsActive BIT NOT NULL DEFAULT 1, -- 1 = active, 0 = inactive
    
    -- Khóa ngoại tự tham chiếu (cho danh mục cha-con)
    CONSTRAINT FK_Category_Parent FOREIGN KEY (ParentCategoryID) REFERENCES Category(CategoryID)
);
GO

-- 3. Bảng Tag
CREATE TABLE Tag (
    TagID INT PRIMARY KEY IDENTITY(1,1),
    TagName NVARCHAR(50) NOT NULL UNIQUE,
    Note NVARCHAR(200)
);
GO

-- 4. Bảng NewsArticle
CREATE TABLE NewsArticle (
    NewsArticleID INT PRIMARY KEY IDENTITY(1,1),
    NewsTitle NVARCHAR(255) NOT NULL,
    Headline NVARCHAR(500),
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE(),
    NewsContent NVARCHAR(MAX) NOT NULL,
    NewsSource NVARCHAR(100),
    CategoryID INT NOT NULL,
    NewsStatus TINYINT NOT NULL DEFAULT 0, -- 0 = inactive, 1 = active
    CreatedByID INT NOT NULL,
    UpdatedByID INT NULL,
    ModifiedDate DATETIME NULL,
    
    -- Khóa ngoại
    CONSTRAINT FK_NewsArticle_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID),
    CONSTRAINT FK_NewsArticle_CreatedBy FOREIGN KEY (CreatedByID) REFERENCES SystemAccount(AccountID),
    CONSTRAINT FK_NewsArticle_UpdatedBy FOREIGN KEY (UpdatedByID) REFERENCES SystemAccount(AccountID)
);
GO

-- 5. Bảng NewsTag (Bảng nối Many-to-Many)
CREATE TABLE NewsTag (
    NewsArticleID INT NOT NULL,
    TagID INT NOT NULL,
    
    -- Khóa chính tổng hợp
    PRIMARY KEY (NewsArticleID, TagID),
    
    -- Khóa ngoại
    CONSTRAINT FK_NewsTag_NewsArticle FOREIGN KEY (NewsArticleID) REFERENCES NewsArticle(NewsArticleID),
    CONSTRAINT FK_NewsTag_Tag FOREIGN KEY (TagID) REFERENCES Tag(TagID)
);
GO

/*
----------------------------------------------------------------
-- CHÈN DỮ LIỆU MẪU (SAMPLE DATA)
-- Chèn dữ liệu theo đúng thứ tự đã tạo bảng
----------------------------------------------------------------
*/

-- 1. Dữ liệu cho SystemAccount
INSERT INTO SystemAccount (AccountName, AccountEmail, AccountPassword, AccountRole) VALUES
(N'Admin User', 'admin@example.com', 'OjPKoiQjDRrVdhfCMtuxCkisBI8Lrh7nmBoOz624/Vg=', 0), -- Giả sử Admin là 0
(N'Staff Writer', 'staff@example.com', 'OjPKoiQjDRrVdhfCMtuxCkisBI8Lrh7nmBoOz624/Vg=', 1), -- Staff
(N'Lecturer User', 'lecturer@example.com', 'OjPKoiQjDRrVdhfCMtuxCkisBI8Lrh7nmBoOz624/Vg=', 2); -- Lecturer
GO

-- 2. Dữ liệu cho Category
INSERT INTO Category (CategoryName, CategoryDescription, ParentCategoryID, IsActive) VALUES
(N'Thời sự', N'Tin tức thời sự trong nước và quốc tế', NULL, 1), -- ID = 1
(N'Công nghệ', N'Tin tức về công nghệ, khoa học', NULL, 1), -- ID = 2
(N'Thể thao', N'Tin tức thể thao', NULL, 1), -- ID = 3
(N'Tin quốc tế', N'Tin tức thời sự quốc tế', 1, 1), -- ID = 4 (Con của Thời sự)
(N'Tin trong nước', N'Tin tức thời sự trong nước', 1, 1); -- ID = 5 (Con của Thời sự)
GO

-- 3. Dữ liệu cho Tag
INSERT INTO Tag (TagName, Note) VALUES
(N'AI', N'Trí tuệ nhân tạo'), -- ID = 1
(N'SQL Server', N'Database'), -- ID = 2
(N'Việt Nam', N'Tin trong nước'), -- ID = 3
(N'Bóng đá', NULL), -- ID = 4
(N'Startup', NULL); -- ID = 5
GO

-- 4. Dữ liệu cho NewsArticle
-- Sử dụng AccountID '2' (Staff Writer) làm người tạo
INSERT INTO NewsArticle (NewsTitle, Headline, NewsContent, NewsSource, CategoryID, NewsStatus, CreatedByID) VALUES
(
    N'AI tạo sinh thay đổi ngành công nghệ',
    N'Các mô hình AI mới đang được ra mắt hàng tuần',
    N'Nội dung chi tiết về việc AI tạo sinh (Generative AI) đang thay đổi cách chúng ta làm việc và sáng tạo...',
    N'Tech News',
    2, -- Category 'Công nghệ'
    1, -- Active
    2  -- Created by Staff Writer
), -- ID = 1
(
    N'Đội tuyển Việt Nam chuẩn bị cho vòng loại World Cup',
    N'HLV trưởng đã công bố danh sách triệu tập',
    N'Nội dung chi tiết về kế hoạch tập luyện và các cầu thủ được gọi...',
    N'Sports Daily',
    3, -- Category 'Thể thao'
    1, -- Active
    2  -- Created by Staff Writer
), -- ID = 2
(
    N'Hội nghị thượng đỉnh kinh tế thế giới',
    N'Các nhà lãnh đạo thảo luận về lạm phát',
    N'Nội dung chi tiết về hội nghị...',
    N'World Times',
    4, -- Category 'Tin quốc tế'
    0, -- Inactive (bản nháp)
    2  -- Created by Staff Writer
); -- ID = 3
GO

-- 5. Dữ liệu cho NewsTag (Liên kết bài viết và thẻ)
INSERT INTO NewsTag (NewsArticleID, TagID) VALUES
(1, 1), -- Bài viết 'AI' -> Tag 'AI'
(1, 5), -- Bài viết 'AI' -> Tag 'Startup'
(2, 3), -- Bài viết 'Bóng đá' -> Tag 'Việt Nam'
(2, 4), -- Bài viết 'Bóng đá' -> Tag 'Bóng đá'
(3, 3); -- Bài viết 'Kinh tế' -> Tag 'Việt Nam'
GO

-- Kiểm tra nhanh dữ liệu
SELECT A.NewsTitle, C.CategoryName, S.AccountName AS Author, T.TagName
FROM NewsArticle AS A
JOIN Category AS C ON A.CategoryID = C.CategoryID
JOIN SystemAccount AS S ON A.CreatedByID = S.AccountID
LEFT JOIN NewsTag AS NT ON A.NewsArticleID = NT.NewsArticleID
LEFT JOIN Tag AS T ON NT.TagID = T.TagID;
GO