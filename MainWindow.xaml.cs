using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Windows;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SmartPurchase
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // WebDriverManager를 사용하여 ChromeDriver를 설치 및 설정
            new DriverManager().SetUpDriver(new ChromeConfig());
        }

        // 독립함수 : 더보기 버튼 반복클릭
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
                    string message = ex.Message.Replace("'", "\\'");
                    js.ExecuteScript($"alert('에러: {message}');");
                    break;
                }
            }
        }

        // 독립함수 : 구매내역 메모장으로 저장
        public void SavePurchaseHistory(object tables, IJavaScriptExecutor js)
        {
            // 결과 저장용 StringBuilder
            StringBuilder sb = new StringBuilder();

            // 구매내역이 있을 때
            if (tables != null)
            {
                foreach (var table in (IEnumerable<dynamic>)tables)
                {
                    try
                    {
                        var dateNode = table.SelectSingleNode(".//thead//span[contains(text(), '20')]");
                        string orderDate = dateNode?.InnerText.Trim() ?? "날짜 없음";

                        var productNode = table.SelectSingleNode(".//tbody//span[contains(@class, 'text-bold')]");
                        string productName = productNode?.InnerText.Trim() ?? "상품명 없음";

                        sb.AppendLine($"구매일자: {orderDate} / 상품명: {productName}");
                    }
                    catch (Exception ex)
                    {
                        js.ExecuteScript($"alert('에러가 발생했습니다. {ex}');");
                    }
                }
                // 바탕화면으로 저장하기
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string filePath = Path.Combine(desktopPath, "구매내역.txt");
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                js.ExecuteScript("alert('바탕화면에 저장되었습니다.');");
            }
            // 구매내역이 없을때
            else
            {
                js.ExecuteScript("alert('구매내역이 없습니다.');");
            }
        }


        // 버튼 클릭 시 동작
        private void RunAutomationButton_Click(object sender, RoutedEventArgs e)
        {
            // ChromeDriverService 생성
            var service = ChromeDriverService.CreateDefaultService();
            service.HideCommandPromptWindow = true; // 반드시 true로
            service.SuppressInitialDiagnosticInformation = true; // 추가로 진단 정보 출력도 억제

            // ChromeDriver 실행
            using (IWebDriver driver = new ChromeDriver(service))
            {
                try
                {
                    // 쇼핑몰로 이동
                    driver.Navigate().GoToUrl("https://modellisti01.imweb.me/");

                    // 페이지 로딩 후 JavaScript로 sessionStorage에서 'oms_token' 값을 가져옴
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                    // 타임아웃(예: 3분 = 180초) 제한
                    int maxCheckSecond = 180;
                    int waitedSecond = 0;
                    bool isLoggedIn = false;

                    while (waitedSecond < maxCheckSecond)
                    {
                        string omsToken = (string)js.ExecuteScript("return sessionStorage.getItem('oms_token');");
                        // 로그인 되어 있을 시
                        if (!string.IsNullOrEmpty(omsToken))
                        {
                            // js.ExecuteScript("alert('로그인중');");
                            isLoggedIn = true;

                            // 구매내역 페이지로 이동
                            driver.Navigate().GoToUrl("https://modellisti01.imweb.me/shop_mypage");

                            // 페이지 완전 로드 대기 (2초)
                            Thread.Sleep(2000);

                            // 독립함수 호출 : 구매내역에서 '더보기' 계속 클릭
                            ClickAllMoreButtons(driver, js);

                            var tables = driver.FindElements(By.XPath("//table[contains(@class, 'im-order-list-table')]"));

                            // 독립함수 호출 : 내용 추출하여 바탕화면으로 출력
                            SavePurchaseHistory(tables, js);

                            break;
                        }
                        Thread.Sleep(2000); // 2초 간격으로 다시 체크
                        waitedSecond += 2;
                    }
                    // 로그인 상태 아닐 시
                    if (!isLoggedIn)
                    {
                        // js.ExecuteScript("alert('로그인 필요 : 시간 초과');");
                    }

                    Thread.Sleep(2000); // 알림 보기용 대기시간
                }
                finally
                {
                    // 브라우저 창 유지
                    MessageBox.Show("브라우저 창이 열려있습니다. 닫으려면 확인을 누르세요.");
                    // 브라우저 닫기
                    driver.Quit();

                }
            }
        }
    }
}