using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Windows;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System.Threading;

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
                            js.ExecuteScript("alert('로그인중');");
                            isLoggedIn = true;
                            break;
                        }
                        Thread.Sleep(2000); // 2초 간격으로 다시 체크
                        waitedSecond += 2;
                    }
                    // 로그인 상태 아닐 시
                    if (!isLoggedIn)
                    {
                        js.ExecuteScript("alert('로그인 필요 : 시간 초과');");
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
