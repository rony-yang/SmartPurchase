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
using OpenQA.Selenium.Support.UI;

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
                MessageBox.Show($"인터넷 연결을 확인 해 주세요.\n\n" + ex.Message);
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

        // 구매내역 메모장으로 저장
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


        // 1. 사용자가 "버튼"을 누르면 크롬 브라우저로 바로 연결되면서 쇼핑몰로 이동
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

                    // 'oms_token'값으로 로그인 확인 : 1-1-1. 사용자가 직접 로그인을 하도록 대기
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;

                    // 로그인 대기 시간 타임아웃(예: 3분 = 180초) 제한
                    int maxCheckSecond = 180;
                    int waitedSecond = 0;
                    bool isLoggedIn = false;
                    
                    // 3분 동안 2초마다 토큰 값 확인
                    while (waitedSecond < maxCheckSecond)
                    {
                        string omsToken = (string)js.ExecuteScript("return sessionStorage.getItem('oms_token');");
                        // 1-1-1-1. 로그인 성공
                        if (!string.IsNullOrEmpty(omsToken))
                        {
                            isLoggedIn = true;

                            // 구매내역 페이지로 이동
                            driver.Navigate().GoToUrl("https://modellisti01.imweb.me/shop_mypage");

                            // 페이지 완전 로드 대기 (2초)
                            Thread.Sleep(2000);

                            // 1-1-1-1-1. '더보기 버튼' 자동 클릭하여 과거 구매내역까지 전체 출력
                            ClickAllMoreButtons(driver, js);

                            var tables = driver.FindElements(By.XPath("//table[contains(@class, 'im-order-list-table')]"));
                            System.Diagnostics.Debug.WriteLine($"테이블 개수: {tables.Count}");

                            // 1-1-1-1-1-1. 구매내역 출력 성공
                            if (tables.Count > 0) {

                            }
                            // 1-1-1-1-1-2. 구매내역 출력 실패
                            else
                            {

                            }

                            // 1-1-1-1-1-1-1. 전체 내역에서 필요한 정보(구매일, 상품명)만 추출
                            // SavePurchaseHistory(tables, js);

                            // 1-1-1-1-1-1-1. 정보 추출 성공
                            /*
                            if ()
                            {
                                // 1-1-1-1-1-1-1-1. 추출한 정보를 메모장으로 출력하여 바탕화면으로 저장

                                // 1-1-1-1-1-1-1-1-1. 바탕화면 저장 성공
                                // 1-1-1-1-1-1-1-1-2. 바탕화면 저장 실패(윈도우 환경 아님, 경로 지정 실패, 파일명 중복 등)
                            }
                            // 1-1-1-1-1-1-2. 정보 추출 실패(구매내역 미존재 등)
                            else
                            {

                            }
                            */

                            break;
                        }

                        Thread.Sleep(2000); // 2초 간격으로 다시 체크
                        waitedSecond += 2;
                    }
                    // 1-1-1-2. 로그인 실패(아이디 또는 비밀번호 불일치, 입력시간 초과 등)
                    if (!isLoggedIn)
                    {
                        MessageBox.Show("로그인 대기 시간이 지났습니다.");
                        // 1-1-1-2-1. 재로그인 시도 시 성공 👉 1-1-1-1. 로그인 성공 으로 이동
                        // 1-1-1-2-2. 재로그인 시도 후에도 다시 실패
                    }

                    Thread.Sleep(2000); // 알림 보기용 대기시간
                }
                // 1-2. 쇼핑몰로 이동 실패(인터넷 미연결, 크롬 미설치, 쇼핑몰 서버 일시적 다운 등)
                catch (Exception ex)
                {
                    MessageBox.Show($"연결에 실패했습니다.\n\n" + ex.Message);
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