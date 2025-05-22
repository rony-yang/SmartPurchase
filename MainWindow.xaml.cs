using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Windows;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Documents;
using System.Linq;

namespace SmartPurchase
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            try {
                // WebDriverManager를 사용하여 ChromeDriver를 설치 및 설정
                // 프로그램 실행마다 설치 여부를 체크하고, 없거나 업데이트 필요시에만 설치 진행
                new DriverManager().SetUpDriver(new ChromeConfig());
            } catch (Exception ex) {
                MessageBox.Show($"인터넷 연결을 확인 해 주세요.\n" + ex.Message);
            }
        }

        // 더보기 버튼 반복클릭
        public void ClickAllMoreButtons(IWebDriver driver, IJavaScriptExecutor js)
        {
            while (true)
            {
                try
                {
                    // 더보기 버튼 탐색
                    var moreButton = driver.FindElement(By.Id("shop_mypage_orderlist_more"));

                    // 버튼이 화면에 표시되는 상태인지 확인
                    if (moreButton.Displayed && moreButton.Enabled)
                    {
                        moreButton.Click();
                        // 데이터 로딩 대기 (2초)
                        Thread.Sleep(2000);
                        // 상품명, 주문일자 가져오기
 
                    }
                    else
                    {
                        // 더 이상 버튼이 없음
                        break;
                    }
                }
                catch (NoSuchElementException)
                {
                    // 더보기 버튼을 찾지 못했을 경우 반복 종료
                    break;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"에러가 발생했습니다.\n" + ex.Message);
                    break;
                }
            }
        }

        // 테이블에서 상품명, 주문일자 가져오기
        public List<(string OrderDate, string ProductName)> ExtractOrderDetails(IWebDriver driver)
        {
            var result = new List<(string, string)>();

            var tables = driver.FindElements(By.CssSelector("table")); // 테이블 전체
            foreach (var table in tables)
            {
                try
                {
                    string orderDate = table.FindElement(By.CssSelector("thead span.im-xs-bold")).Text.Trim();
                    var productElements = table.FindElements(By.CssSelector("tbody .im-body-size.im-body-line-height.text-bold"));

                    foreach (var product in productElements)
                    {
                        string productName = product.Text.Trim();
                        if (!string.IsNullOrEmpty(productName))
                        {
                            result.Add((orderDate, productName));
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("예외 발생: " + ex.Message);
                }
            }

            return result;
        }


        // 구매내역 메모장으로 추출해서 바탕화면으로 저장
        public void SavePurchaseHistory(List<(string OrderDate, string ProductName)> records)
        {
            StringBuilder sb = new StringBuilder();

            if (records != null && records.Any())
            {
                foreach (var record in records)
                {
                    sb.AppendLine($"구매일자: {record.OrderDate} / 상품명: {record.ProductName}");
                }

                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string filePath = Path.Combine(desktopPath, "구매내역.txt");

                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                System.Diagnostics.Debug.WriteLine("구매내역 저장 완료");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("저장할 구매내역이 없습니다.");
            }
        }


        // 사용자가 "버튼"을 누르면 크롬 브라우저로 바로 연결되면서 쇼핑몰로 이동
        private void RunAutomationButton_Click(object sender, RoutedEventArgs e)
        {
            // ChromeDriverService 생성
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true; // 반드시 true로
            service.SuppressInitialDiagnosticInformation = true; // 추가로 진단 정보 출력도 억제

            // ChromeDriver 실행
            using (IWebDriver driver = new ChromeDriver(service))
            {
                // 1-1. 쇼핑몰 이동 성공
                try
                {
                    // 쇼핑몰로 이동
                    driver.Navigate().GoToUrl("https://modellisti01.imweb.me/");

                    // Thread.Sleep(2000); // 페이지 로딩 대기

                    // 로그인 버튼 자동 클릭 (팝업 열기)
                    IWebElement loginButton = driver.FindElement(By.XPath("//a[contains(@onclick, 'SITE_MEMBER.openLogin')]"));
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    js.ExecuteScript("arguments[0].click();", loginButton);

                    // 'oms_token'값으로 로그인 확인 : 사용자가 직접 로그인을 하도록 대기
                    // IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                    // 로그인 대기 시간 타임아웃(예: 3분 = 180초) 제한
                    int maxCheckSecond = 180;
                    int waitedSecond = 0;
                    bool isLoggedIn = false;
                    
                    // 3분 동안 2초마다 토큰 값 확인
                    while (waitedSecond < maxCheckSecond)
                    {
                        string omsToken = (string)js.ExecuteScript("return sessionStorage.getItem('oms_token');");
                        // 로그인 성공
                        if (!string.IsNullOrEmpty(omsToken))
                        {
                            isLoggedIn = true;

                            // 구매내역 페이지로 이동
                            driver.Navigate().GoToUrl("https://modellisti01.imweb.me/shop_mypage");

                            // 페이지 완전 로드 대기 (2초)
                            Thread.Sleep(2000);

                            // '더보기 버튼' 자동 클릭하여 과거 구매내역까지 전체 출력
                            ClickAllMoreButtons(driver, js);

                            // 전체 페이지에서 내용 추출
                            var records = this.ExtractOrderDetails(driver);

                            // 추출한 정보를 메모장으로 출력하여 바탕화면으로 저장
                            this.SavePurchaseHistory(records);

                            break;
                        }

                        Thread.Sleep(2000); // 2초 간격으로 다시 체크
                        waitedSecond += 2;
                    }
                    // 로그인 실패(아이디 또는 비밀번호 불일치, 입력시간 초과 등)
                    if (!isLoggedIn)
                    {
                        MessageBox.Show("로그인 대기 시간이 지났습니다.");
                    }

                    Thread.Sleep(2000); // 알림 보기용 대기시간
                }
                // 쇼핑몰로 이동 실패(인터넷 미연결, 크롬 미설치, 쇼핑몰 서버 일시적 다운 등)
                catch (Exception ex)
                {
                    MessageBox.Show($"연결에 실패했습니다.\n" + ex.Message);
                }
                finally
                {
                    // 브라우저 창 유지
                    // MessageBox.Show("브라우저 창이 열려있습니다. 닫으려면 확인을 누르세요.");
                    // 작업이 끝나면 브라우저 자동 종료
                    driver.Quit();

                }
            }
        }
    }
}