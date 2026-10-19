# GNN Tabanlı Gaz Kaçağı Risk Puanlaması

Sınav sahnesinde kullanıcının durduğu noktanın **güvenli**, **dikkat** veya **tehlikeli** olduğunu
eğitilmiş bir **GraphSAGE** modeli belirler. `GasCollider` (GasWrongBehaviour) trigger kutusu artık
puanlamada kullanılmıyor, yalnızca eğitim etiketi olarak işe yarıyor (GNN puanlayıcı yoksa veya model
yüklenemezse eski kutu cezası otomatik devreye girer).

## Akış

1. **Graf** (`Assets/_Scripts/GNN/GnnGraphBuilder.cs`): Depo zemini 0.5 m'lik hücrelere bölünür.
   Her yürünebilir ve boş zemin hücresi bir düğümdür. Arada engel olmayan 8 komşu hücre kenarla bağlanır.
   Alan, gaz kutusunun çevresi 40 m genişletilerek bulunur (`GnnRiskPredictor > Site Area` ile elle verilebilir).
2. **Düğüm özellikleri** (7 adet, yalnızca geometriden):
   - gaz kaynağına düz uzaklık (`SceneManager > Gas Particle` konumu)
   - gaz kaynağına **zemin üzerinden** uzaklık: graf kenarları boyunca en kısa yol, gaz duvardan geçmez
   - tavan yüksekliği (gaz kapalı alanda birikir), çevre açıklığı, açık yön oranı (havalanma)
   - en yakın kapı / çıkışa uzaklık (obje adlarından: `door`, `exit` …)
3. **Etiket**: Gaz kutusunun (yatayda) içi `Danger`, kutuya 2.5 m'den yakın `Caution`, kalan alan `Safe`.
4. **Eğitim** (`GNN/train_gnn.py`): 2 katmanlı GraphSAGE (mean aggregator) + softmax.
   Veri 2 m'lik uzamsal bloklara göre bölünür, sınıf dengesizliği için ağırlıklı cross-entropy kullanılır.
5. **Çalışma zamanı**: Depo sabit olduğu için graf Editor'de bir kez çıkarılıp
   `Assets/Resources/GNN/gnn_graph_<sahne>.json` olarak kaydedilir. Oyunda sahne taranmaz;
   `GnnRiskPredictor` modeli sahne açılınca **bir kez** çalıştırır. `GnnRiskScorer` gaz kaçağı sürerken
   her 0.5 s'de oyuncunun (ana kamera) düğümünü sorgular:
   - Gaz bütçesi 40 puan (Fine-Kinney şiddet ölçeği: zehirlenme / patlama öldürücü).
   - Toplam gaz cezası: `40 × [0.4 × w_giriş + 0.6 × (1 − e^(−E / 5 s))]`
     - `w_giriş`: ilk girişteki GNN tehlike olasılığı (girişte en fazla 16 puan hemen kesilir)
     - `E`: gaz bölgesinde geçen, GNN tehlike olasılığıyla ağırlıklandırılmış süre
   - Ceza bütçeyi aşamaz; çıkıp girmek giriş payını tekrar kesmez, süre birikmeye devam eder.
   - Dikkat bölgesi varsayılan olarak yalnızca uyarıdır (`Caution Penalty` = 0, `Warning Sound` atanırsa ses çalar).
   - Vana kapatılınca (gaz bitince) puanlama durur.
6. **Sınav puanı** (`CalculateExamSceneScore`, 100'den düşülür). Her hatanın şiddetine göre bir bütçesi var:

   | Hata | Ceza |
   |---|---|
   | Müdahale süresi (gaz başlangıcı → vana kapanması) | İlk 30 sn cezasız, sonrası `30 × (1 − e^(−gecikme / 30 s))`, en fazla 30 |
   | Gaz bulutuna girme (GNN) | Yukarıdaki formül, en fazla 40 |
   | Gaz varken elektrikli kapı butonuna basma (kıvılcım) | İlk basış 30, sonrakiler +10, en fazla 50 |
   | Alarm vermeden tahliye | 30 |

   Sahne sonunda puanın altında standart risk etiketi gösterilir:
   75+ Güvenli, 50–74 Orta Riskli, 25–49 Riskli, 25 altı Yüksek Riskli.

   | Senaryo | Puan | Etiket |
   |---|---|---|
   | Kusursuz: vana 25 sn, alarm verdi | 100 | Güvenli |
   | Yavaş ama doğru: vana 50 sn | 85 | Güvenli |
   | Çok yavaş: vana 90 sn | 74 | Orta Riskli |
   | Alarm vermeden tahliye | 70 | Orta Riskli |
   | Gaz varken kapı butonuna 1 kez bastı | 70 | Orta Riskli |
   | Gaz bulutundan 1 sn geçti | 80 | Güvenli |
   | Gaz bulutunda uzun kaldı | 60 | Orta Riskli |
   | Gaz bulutunda uzun + kapı butonu | 25 | Riskli |
   | Çok yavaş + alarmsız | 44 | Riskli |
   | Gaz uzun + kapı + alarmsız | 0 | Yüksek Riskli |

   Tepki süresi (30 sn), başlangıç noktasından vanaya yürüyüp kapatma süresine göre ayarlanmalıdır:
   deneyimli birinin süresini ölçüp yaklaşık 1,5 katını `Response Grace Period` olarak girin.

## Kurulum

```bash
# 1) Unity: VR-ASYS-GAZ-SINAV sahnesini aç
#    Hierarchy > Create Empty > adı "GNN" > Add Component: GnnRiskPredictor ve GnnRiskScorer > sahneyi kaydet
#    Tools > GNN > Eğitim Verisini ve Grafı Dışa Aktar (Açık Sahne)
#      → GNN/data/VR-ASYS-GAZ-SINAV_graph_{0..3}.json (eğitim)
#      → Assets/Resources/GNN/gnn_graph_VR-ASYS-GAZ-SINAV.json (oyun)

# 2) Eğit
python3 -m venv GNN/.venv && source GNN/.venv/bin/activate
pip install -r GNN/requirements.txt
python3 GNN/train_gnn.py            # --onnx ile ONNX da üretir

# 3) Assets/Resources/GNN/gnn_weights.json oluşur → Unity otomatik yükler
```

Görsel kontrol için sahnedeki `GnnRiskPredictor` bileşenine sağ tık:
- **Grafı kur ve etiketleri göster**: eğitim etiketleri (yeşil / sarı / kırmızı)
- **Kayıtlı grafla modeli çalıştır**: model kararı (yeşil Güvenli, sarı Dikkat, kırmızı Tehlikeli)

Bu görüntüleri görmek için objenin seçili olması gerekir (gizmo).

Metrikler `GNN/report/metrics.json` dosyasına yazılır:
- 5-katlı uzamsal çapraz doğrulama (ortalama ± std)
- Sınıf bazlı P/R/F1 ve karışıklık matrisi
- 1-komşu toleranslı metrikler (etiket kutusunun kenar hatalarını ayırmak için)

## Bilinen sınırlama

Sahnede yalnızca 1 küçük gaz kutusu (yaklaşık 4 × 7 m) var; bu, grafta ~120 tehlikeli düğüm demek.
"Tehlikeli" sınıfının çapraz doğrulama skoru bu yüzden katlar arasında çok oynar (std yüksek çıkar).
Daha güvenilir sonuç için eğitim sahnesindeki (VR-ASYS-GAZ) gaz kutusu da dışa aktarılıp eğitime eklenebilir
ya da kutu gerçek gaz yayılım alanına göre büyütülebilir.

> Grid ayarları (`GnnGraphSettings`) veya özellik kodu değişirse veriyi yeniden dışa aktarıp
> modeli yeniden eğitin. Model, özellik sırası uyuşmazsa yüklenmeyi reddeder.
